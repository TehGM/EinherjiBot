using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using TehGM.EinherjiBot.CommandsProcessing;

namespace TehGM.EinherjiBot.EliteDangerous
{
    [LoadRegexCommands]
    [PersistentModule(PreInitialize = true)]
    public class CommunityGoalsHandler : IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<CommunityGoalsOptions> _options;
        private readonly ICommunityGoalsHistoryStore _cgHistoryStore;
        private CancellationTokenSource _autoModeCTS;
        private IEnumerable<CommunityGoal> _cgCache;
        private DateTime _cacheUpdateTimeUtc;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public CommunityGoalsHandler(DiscordSocketClient client, IHttpClientFactory httpClientFactory, ILogger<CommunityGoalsHandler> log, ICommunityGoalsHistoryStore cgHistoryStore,
            IOptionsMonitor<CommunityGoalsOptions> options)
        {
            this._client = client;
            this._httpClientFactory = httpClientFactory;
            this._log = log;
            this._cgCache = Enumerable.Empty<CommunityGoal>();
            this._cgHistoryStore = cgHistoryStore;
            this._options = options;

            if (this._client.ConnectionState == ConnectionState.Connected)
                StartAutomaticNewsPosting();

            this._client.Ready += OnClientReady;
            this._client.Connected += OnClientConnected;
            this._client.Disconnected += OnClientDisconnected;
        }

        private Task OnClientConnected()
            => OnClientReady();

        private Task OnClientReady()
        {
            if (this._client.ConnectionState == ConnectionState.Connected && this._client.CurrentUser != null)
                StartAutomaticNewsPosting();
            return Task.CompletedTask;
        }

        private Task OnClientDisconnected(Exception arg)
        {
            StopAutomaticNewsPosting();
            return Task.CompletedTask;
        }

        private void StartAutomaticNewsPosting()
        {
            if (_autoModeCTS != null)
                return;

            _autoModeCTS = new CancellationTokenSource();
            Task autoTask = Task.Run(async () =>
            {
                using IDisposable context = _log.UseSource("Elite CGs");
                CancellationToken cancellationToken = _autoModeCTS.Token;
                // wait 5 seconds to let the client get connection state in check
                await Task.Delay(5 * 1000, cancellationToken).ConfigureAwait(false);
                _log.LogDebug("Starting automatic ED CG checker");
                DateTime _lastRetrievalTime = DateTime.MinValue;
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        TimeSpan nextUpdateIn = (_lastRetrievalTime + _options.CurrentValue.AutoNewsInterval) - DateTime.UtcNow;
                        // if still waiting, await time, and repeat iteration
                        if (nextUpdateIn > TimeSpan.Zero)
                        {
                            _log.LogTrace("Next update in: {TimeRemaining}", nextUpdateIn);
                            // this will not reflect on updates to options monitor, but that's ok
                            await Task.Delay(nextUpdateIn, cancellationToken).ConfigureAwait(false);
                            continue;
                        }

                        CommunityGoalsOptions options = _options.CurrentValue;

                        // get guild channel
                        if (!(_client.GetChannel(options.AutoNewsChannelID) is SocketTextChannel guildChannel))
                            throw new InvalidOperationException($"Channel {options.AutoNewsChannelID} is not a valid guild text channel.");

                        // retrieve CG data, take only new or finished ones, and then update cache
                        IEnumerable<CommunityGoal> allCGs = await QueryCommunityGoalsAsync(cancellationToken).ConfigureAwait(false);
                        IList<CommunityGoal> newOrJustFinishedCGs = new List<CommunityGoal>(allCGs.Count());
                        foreach (CommunityGoal cg in allCGs)
                        {
                            CommunityGoal historyCg = await _cgHistoryStore.GetAsync(cg.ID, cancellationToken).ConfigureAwait(false);
                            if (historyCg == null || historyCg.IsCompleted != cg.IsCompleted)
                            {
                                newOrJustFinishedCGs.Add(cg);
                                await _cgHistoryStore.SetAsync(cg, cancellationToken).ConfigureAwait(false);
                            }
                        }
                        _log.LogTrace("New or just finished CGs count: {Count}", newOrJustFinishedCGs.Count);

                        // post all CGs
                        _log.LogTrace("Sending CGs");
                        foreach (CommunityGoal cg in newOrJustFinishedCGs)
                            await guildChannel.SendMessageAsync(null, false, CommunityGoalToEmbed(cg).Build(), cancellationToken).ConfigureAwait(false);
                        _lastRetrievalTime = DateTime.UtcNow;
                    }
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) when (ex.LogAsError(_log, "Error occured in automatic ED CG checker loop")) { }
                finally
                {
                    _log.LogDebug("Stopping automatic ED CG checker");
                    // clear CTS on exiting if it wasn't cleared yet
                    if (_autoModeCTS?.Token == cancellationToken)
                        _autoModeCTS = null;
                }
            }, _autoModeCTS.Token);
        }

        public void StopAutomaticNewsPosting()
        {
            _autoModeCTS?.Cancel();
            _autoModeCTS = null;
        }

        [RegexCommand("^elite (?:cgs?|community goals?)")]
        private async Task CmdCommunityGoals(SocketCommandContext context, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            IEnumerable<CommunityGoal> cgs = await QueryCommunityGoalsAsync(cancellationToken).ConfigureAwait(false);
            foreach (CommunityGoal cg in cgs)
                await context.ReplyAsync(null, false, CommunityGoalToEmbed(cg).Build(), cancellationToken).ConfigureAwait(false);
        }

        private async Task<IEnumerable<CommunityGoal>> QueryCommunityGoalsAsync(CancellationToken cancellationToken = default)
        {
            CommunityGoalsOptions options = _options.CurrentValue;
            // use cache if it's too early for retrieving again
            if ((DateTime.UtcNow - _cacheUpdateTimeUtc) < options.CacheLifetime)
            {
                _log.LogTrace("CG cache is recent, not updating");
                return _cgCache;
            }

            // build query content
            const string eventName = "getCommunityGoalsRecent";
            JObject query = new JObject();
            query.Add("header", new JObject(
                new JProperty("appName", options.InaraAppName),
                new JProperty("appVersion", options.InaraAppVersion),
                new JProperty("isDeveloped", options.InaraAppInDevelopment),
                new JProperty("APIkey", options.InaraApiKey)));
            JObject eventParams = new JObject(
                new JProperty("eventName", eventName),
                new JProperty("eventTimestamp", DateTimeOffset.Now),
                new JProperty("eventData", new JArray()));
            query.Add("events", new JArray(eventParams));

            // send query and get results
            _log.LogDebug("Sending {EventName} event to Inara", eventName);
            HttpClient client = _httpClientFactory.CreateClient();
            using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, options.InaraURL);
            request.Headers.Add("User-Agent", options.InaraAppName);
            request.Content = new StringContent(query.ToString(Newtonsoft.Json.Formatting.None), Encoding.UTF8, "application/json");
            using HttpResponseMessage response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            // return results
            IEnumerable<JToken> responseObjectsArray = JObject.Parse(await response.Content.ReadAsStringAsync().ConfigureAwait(false))["events"][0]?["eventData"]?.Children() ?? Enumerable.Empty<JToken>();
            _log.LogDebug("Retrieved {ObjectsCount} JSON event data objects from Inara", responseObjectsArray.Count());


            DateTime minDate = DateTime.UtcNow.Add(-options.MaxAge);
            IEnumerable<CommunityGoal> results = responseObjectsArray
                .Select(cgJson => cgJson.ToObject<CommunityGoal>())
                .Where(cg => !cg.IsCompleted || cg.ExpirationTime.Date >= minDate);
            _cgCache = results;
            _cacheUpdateTimeUtc = DateTime.UtcNow;
            return results;
        }

        private EmbedBuilder CommunityGoalToEmbed(CommunityGoal cg)
        {
            string thumbnailURL = _options.CurrentValue.ThumbnailURL;
            string descriptionTrimmed = cg.Description.Length <= EmbedBuilder.MaxDescriptionLength ? cg.Description :
                $"{cg.Description.Remove(EmbedBuilder.MaxDescriptionLength - 3)}...";
            EmbedBuilder builder = new EmbedBuilder()
                .WithAuthor("Elite Dangerous Community Goals", _client.CurrentUser.GetAvatarUrl() ?? _client.CurrentUser.GetDefaultAvatarUrl())
                .WithTitle(cg.Name)
                //.WithDescription($"**Objective**: `{cg.Objective}`\n\n{descriptionTrimmed}")
                .WithDescription($"```\n{cg.Objective}\n```\n{descriptionTrimmed}")
                .AddField("System", cg.SystemName, true)
                .AddField("Station", cg.StationName, true)
                .AddField("Tier Reached", $"*{cg.TierReached}* / {cg.TierMax}")
                .AddField("Contributing Pilots", cg.ContributingPilotsCount.ToString(), true)
                .AddField("Contributions Count", cg.ContributionsCount.ToString(), true)
                .AddField("Last Updated", $"{(DateTime.UtcNow - cg.LastUpdateTime.ToUniversalTime()).ToLongFriendlyString()} ago")
                .AddField("Is Completed?", cg.IsCompleted ? "\u2705" : "\u274C", true)
                .WithUrl(cg.InaraURL)
                .WithColor(cg.IsCompleted ? Color.Green : (Color)System.Drawing.Color.Cyan)
                .WithFooter("Powered by Inara | CG expiration time: ")
                .WithTimestamp(cg.ExpirationTime);
            if (!string.IsNullOrWhiteSpace(thumbnailURL))
                builder.WithThumbnailUrl(thumbnailURL);
            if (!cg.IsCompleted)
                builder.AddField("Time Left", (cg.ExpirationTime - DateTimeOffset.Now).ToLongFriendlyString());

            if (!string.IsNullOrWhiteSpace(cg.Reward))
            {
                string rewardTrimmed = cg.Reward.Length <= EmbedFieldBuilder.MaxFieldValueLength ? cg.Reward :
                    $"{cg.Reward.Remove(EmbedFieldBuilder.MaxFieldValueLength - 3)}...";
                builder.AddField("Reward", rewardTrimmed);
            }
            return builder;
        }

        public void Dispose()
        {
            StopAutomaticNewsPosting();
            try { this._client.Ready -= OnClientReady; } catch { }
            try { this._client.Connected -= OnClientConnected; } catch { }
            try { this._client.Disconnected -= OnClientDisconnected; } catch { }
            try { this._lock?.Dispose(); } catch { }
        }
    }
}
