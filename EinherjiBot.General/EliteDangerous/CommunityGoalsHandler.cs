using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using TehGM.EinherjiBot.CommandsProcessing;

namespace TehGM.EinherjiBot.EliteDangerous
{
    [RegexCommandsModule(IsPersistent = true, PreInitialize = true)]
    [HelpCategory("Games", 10)]
    public class CommunityGoalsHandler : IDisposable
    {
        private readonly DiscordClient _client;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<CommunityGoalsOptions> _options;
        private readonly ICommunityGoalsHistoryStore _cgHistoryStore;
        private CancellationTokenSource _autoModeCTS;
        private IEnumerable<CommunityGoal> _cgCache;
        private DateTime _cacheUpdateTimeUtc;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private bool _enabled = false;
        private readonly IDisposable _optionsChangeRegistration;

        private bool IsConnected => this._client.GatewayInfo != null && this._client.CurrentUser != null;

        public CommunityGoalsHandler(DiscordClient client, IHttpClientFactory httpClientFactory, ILogger<CommunityGoalsHandler> log, ICommunityGoalsHistoryStore cgHistoryStore,
            IOptionsMonitor<CommunityGoalsOptions> options)
        {
            this._client = client;
            this._httpClientFactory = httpClientFactory;
            this._log = log;
            this._cgCache = Enumerable.Empty<CommunityGoal>();
            this._cgHistoryStore = cgHistoryStore;
            this._options = options;

            this._enabled = ValidateInaraCredentials();
            if (!this._enabled)
                _log.LogWarning("Inara credentials missing. Elite Dangerous Community Goals feature will be disabled");
            else if (this.IsConnected)
                StartAutomaticNewsPosting();

            this._client.Ready += OnClientReady;
            this._client.SocketOpened += OnClientConnected;
            this._client.SocketClosed += OnClientDisconnected;
            this._optionsChangeRegistration = this._options.OnChange(o =>
            {
                bool previousState = this._enabled;
                this._enabled = ValidateInaraCredentials();
                if (previousState != this._enabled)
                {
                    StopAutomaticNewsPosting();
                    StartAutomaticNewsPosting();
                }
            });
        }

        private Task OnClientConnected(DiscordClient client, SocketEventArgs e)
            => OnClientReady(client, null);

        private Task OnClientReady(DiscordClient client, ReadyEventArgs e)
        {
            if (this._enabled && this.IsConnected)
                StartAutomaticNewsPosting();
            return Task.CompletedTask;
        }

        private Task OnClientDisconnected(DiscordClient client, SocketCloseEventArgs e)
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
                        if (!this._enabled)
                        {
                            _log.LogWarning("Inara credentials missing. Elite Dangerous Community Goals feature will be disabled");
                            return;
                        }

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
                        DiscordChannel channel = await _client.GetChannelAsync(options.AutoNewsChannelID).ConfigureAwait(false);
                        if (!channel.IsGuildText())
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
                            await channel.SendMessageAsync(CommunityGoalToEmbed(cg).Build()).ConfigureAwait(false);
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

        private bool ValidateInaraCredentials()
        {
            CommunityGoalsOptions options = _options.CurrentValue;

            return !string.IsNullOrWhiteSpace(options.InaraApiKey) &&
                !string.IsNullOrWhiteSpace(options.InaraAppName);
        }

        [RegexCommand("^elite (?:cgs?|community goals?)")]
        [Name("elite community goals")]
        [Summary("Shows list of currently ongoing Community Goals in Elite Dangerous.")]
        [Priority(-19)]
        private async Task CmdCommunityGoals(CommandContext context, CancellationToken cancellationToken = default)
        {
            if (!this._enabled)
            {
                _log.LogDebug("Unable to handle Elite Dangerous Community Goals command - Inara credentials missing");
                return;
            }
            IEnumerable<CommunityGoal> cgs = await QueryCommunityGoalsAsync(cancellationToken).ConfigureAwait(false);
            foreach (CommunityGoal cg in cgs)
                await context.ReplyAsync(null, CommunityGoalToEmbed(cg).Build()).ConfigureAwait(false);
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
                new JProperty("appVersion", options.InaraAppVersion ?? BotInfoUtility.GetVersion()),
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

        private DiscordEmbedBuilder CommunityGoalToEmbed(CommunityGoal cg)
        {
            string thumbnailURL = _options.CurrentValue.ThumbnailURL;
            const int maxDescriptionLength = 4096;
            string descriptionTrimmed = cg.Description.Length <= maxDescriptionLength ? cg.Description :
                $"{cg.Description.Remove(maxDescriptionLength - 3)}...";
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                .WithAuthor("Elite Dangerous Community Goals", _client.CurrentUser.GetSafeAvatarUrl(size: 2048))
                .WithTitle(cg.Name)
                .WithDescription($"```\n{cg.Objective}\n```\n{descriptionTrimmed}")
                .AddField("System", cg.SystemName, true)
                .AddField("Station", cg.StationName, true)
                .AddField("Tier Reached", $"*{cg.TierReached}* / {cg.TierMax}")
                .AddField("Contributing Pilots", cg.ContributingPilotsCount.ToString(), true)
                .AddField("Contributions Count", cg.ContributionsCount.ToString(), true)
                .AddField("Last Updated", $"{(DateTime.UtcNow - cg.LastUpdateTime.ToUniversalTime()).ToLongFriendlyString()} ago")
                .AddField("Is Completed?", cg.IsCompleted ? "\u2705" : "\u274C", true)
                .WithUrl(cg.InaraURL)
                .WithColor(cg.IsCompleted ? Color.Green : Color.Cyan)
                .WithFooter("Powered by Inara | CG expiration time: ")
                .WithTimestamp(cg.ExpirationTime);
            if (!string.IsNullOrWhiteSpace(thumbnailURL))
                builder.WithThumbnail(thumbnailURL);
            if (!cg.IsCompleted)
                builder.AddField("Time Left", (cg.ExpirationTime - DateTimeOffset.Now).ToLongFriendlyString());

            if (!string.IsNullOrWhiteSpace(cg.Reward))
            {
                const int maxFieldValueLength = 1024;
                string rewardTrimmed = cg.Reward.Length <= maxFieldValueLength ? cg.Reward :
                    $"{cg.Reward.Remove(maxFieldValueLength - 3)}...";
                builder.AddField("Reward", rewardTrimmed);
            }
            return builder;
        }

        public void Dispose()
        {
            StopAutomaticNewsPosting();
            try { this._optionsChangeRegistration?.Dispose(); } catch { }
            try { this._client.Ready -= OnClientReady; } catch { }
            try { this._client.SocketOpened -= OnClientConnected; } catch { }
            try { this._client.SocketClosed -= OnClientDisconnected; } catch { }
            try { this._lock?.Dispose(); } catch { }
        }
    }
}
