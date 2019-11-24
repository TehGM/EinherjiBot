using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using TehGM.EinherjiBot.CommandsProcessing;
using TehGM.EinherjiBot.Config;
using TehGM.EinherjiBot.DataModels;
using TehGM.EinherjiBot.Extensions;
using TehGM.EinherjiBot.Utilities;

namespace TehGM.EinherjiBot
{
    [ProductionOnly]
    class EliteDangerousHandler : HandlerBase
    {
        private IList<EliteCG> _cgCache = new List<EliteCG>();
        // we need separate cache for auto mode, otherwise users pulling CGs on demand would mess with auto updates
        private IEnumerable<EliteCG> _lastAutoCgs = new List<EliteCG>();
        private DateTime _cacheUpdateTimeUtc;
        private WebClient _webClient = new WebClient();
        private CancellationTokenSource _autoModeCTS;

        public EliteDangerousHandler(DiscordSocketClient client, BotConfig config) : base(client, config)
        {
            _webClient.Headers[HttpRequestHeader.UserAgent] = config.Auth.InaraAPI.AppName;
            _webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

            CommandsStack.Add(new RegexUserCommand("^elite (?:cgs?|community goals?) unsub(?:scribe)?", CmdCommunityGoalsUnsubscribe));
            CommandsStack.Add(new RegexUserCommand("^elite (?:cgs?|community goals?) sub(?:scribe)?", CmdCommunityGoalsSubscribe));
            CommandsStack.Add(new RegexUserCommand("^elite (?:cgs?|community goals?)", CmdCommunityGoals));

            if (Client.ConnectionState == ConnectionState.Connected)
                StartAutomaticNewsPosting();

            Client.Ready += Client_Ready;
            Client.Disconnected += Client_Disconnected;
        }

        private Task Client_Ready()
        {
            StartAutomaticNewsPosting();
            return Task.CompletedTask;
        }

        private Task Client_Disconnected(Exception arg)
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
                using IDisposable context = Logging.UseSource("Inara CGs");
                Logging.Default.Debug("Starting ED automatic CG checker");
                CancellationToken token = _autoModeCTS.Token;
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        TimeSpan nextUpdateIn = (Config.Data.EliteAPI.AutoNewsRetrievalTimeUtc + Config.EliteAPI.EliteAutoNewsInterval) - DateTime.UtcNow;
                        Logging.Default.Verbose("Next update in: {TimeRemaining}", nextUpdateIn);
                        // if still waiting, await time, and repeat iteration
                        if (nextUpdateIn > TimeSpan.Zero)
                        {
                            await Task.Delay(nextUpdateIn, token);
                            continue;
                        }

                        // get guild channel
                        if (!(Client.GetChannel(Config.EliteAPI.EliteAutoNewsChannelID) is SocketTextChannel guildChannel))
                            throw new InvalidOperationException($"Channel {Config.EliteAPI.EliteAutoNewsChannelID} is not a valid guild text channel.");
                        // get collection of subscribers that have to be PMed
                        IEnumerable<ulong> pmSubscribers = Config.EliteAPI.PreferPingOverPM ?
                                Config.Data.EliteAPI.CommunityGoalsSubscribersIDs.Where(uid => guildChannel.GetUser(uid) == null) :
                                Config.Data.EliteAPI.CommunityGoalsSubscribersIDs;
                        // get collection of users to be pinged in guild
                        IEnumerable<ulong> pingSubscribers = pmSubscribers.Count() == Config.Data.EliteAPI.CommunityGoalsSubscribersIDs.Count ?
                                null : Config.Data.EliteAPI.CommunityGoalsSubscribersIDs.Except(pmSubscribers);


                        // with all collections prepared, retrieve CG data, take only new or finished ones, and then update cache
                        IEnumerable<EliteCG> allCGs = await QueryForCGs();
                        IList<EliteCG> newOrJustFinishedCGs = new List<EliteCG>(allCGs.Count());
                        foreach (var cg in allCGs)
                        {
                            EliteCG lastCG = _lastAutoCgs.FirstOrDefault(ecg => ecg.Equals(cg));
                            if (lastCG == null || lastCG.IsCompleted != cg.IsCompleted)
                                newOrJustFinishedCGs.Add(cg);
                        }
                        Logging.Default.Verbose("New or just finished CGs count: {Count}", newOrJustFinishedCGs.Count);
                        _cgCache = allCGs.ToList();
                        _lastAutoCgs = allCGs;
                        _cacheUpdateTimeUtc = DateTime.UtcNow;

                        // post all CGs
                        bool firstPost = true;
                        Logging.Default.Debug("Sending CGs");
                        foreach (var cg in newOrJustFinishedCGs)
                        {
                            Embed cgEmbed = cg.ToEmbed(Config.EliteAPI.ThumbnailURL, Client.CurrentUser.GetAvatarUrl() ?? Client.CurrentUser.GetDefaultAvatarUrl())
                                    .Build();

                            // post in channel first, pinging all those who can be pinged
                            await guildChannel.SendMessageAsync(pingSubscribers == null || !firstPost ? null :
                                    string.Join(' ', pingSubscribers.Select(uid => MentionUtils.MentionUser(uid))), false, cgEmbed);
                            firstPost = false;
                            // pm each pm subscriber
                            foreach (var pmID in pmSubscribers)
                            {
                                IUser pmUser = await Client.GetUserAsync(pmID);
                                IDMChannel pmChannel = await pmUser.GetOrCreateDMChannelAsync();
                                await pmChannel.SendMessageAsync(null, false, cgEmbed);
                            }
                        }


                        // finally, update last checked time
                        Config.Data.EliteAPI.AutoNewsRetrievalTimeUtc = DateTime.UtcNow;
                        await Config.Data.SaveAsync();
                    }
                }
                finally
                {
                    Logging.Default.Debug("Stopping ED automatic CG checker");
                    // clear CTS on exiting if it wasn't cleared yet
                    if (_autoModeCTS?.Token == token)
                        _autoModeCTS = null;
                }
            }, _autoModeCTS.Token);
        }

        public void StopAutomaticNewsPosting()
        {
            _autoModeCTS?.Cancel();
            _autoModeCTS = null;
        }

        private async Task CmdCommunityGoalsSubscribe(SocketCommandContext message, Match match)
        {
            if (Config.Data.EliteAPI.AddCommunityGoalsSubscriber(message.User))
            {
                await Config.Data.SaveAsync();
                await message.ReplyAsync($"{Config.DefaultConfirm} You will now get pinged or PMed when Elite Dangerous' Community Goals are updated.");
            }
            else
                await message.ReplyAsync($"{Config.DefaultReject} You already are subscribed to Elite Dangerous' Community Goals updates.");
        }

        private async Task CmdCommunityGoalsUnsubscribe(SocketCommandContext message, Match match)
        {
            if (Config.Data.EliteAPI.RemoveCommunityGoalsSubscriber(message.User))
            {
                await Config.Data.SaveAsync();
                await message.ReplyAsync($"{Config.DefaultConfirm} You'll no longer get pinged or PMed when Elite Dangerous' Community Goals are updated.");
            }
            else
                await message.ReplyAsync($"{Config.DefaultReject} You are not subscribed to Elite Dangerous' Community Goals updates.");
        }

        private async Task CmdCommunityGoals(SocketCommandContext message, Match match)
        {
            _cgCache = (await QueryForCGs()).ToList();
            _cacheUpdateTimeUtc = DateTime.UtcNow;
            for (int i = 0; i < _cgCache.Count; i++)
                await message.ReplyAsync(null, false, _cgCache[i].ToEmbed(Config.EliteAPI.ThumbnailURL, Client.CurrentUser.GetAvatarUrl() ?? Client.CurrentUser.GetDefaultAvatarUrl())
                    .Build());
        }

        private async Task<IEnumerable<EliteCG>> QueryForCGs()
        {
            // use cache if it's too early for retrieving again
            if ((DateTime.UtcNow - _cacheUpdateTimeUtc) < Config.EliteAPI.CachedCGLifetime)
            {
                Logging.Default.Debug("CG cache is recent, not updating");
                return _cgCache;
            }

            // build query content
            const string eventName = "getCommunityGoalsRecent";
            JObject query = new JObject();
            query.Add("header", JToken.FromObject(Config.Auth.InaraAPI));
            JObject eventParams = new JObject();
            eventParams.Add("eventName", eventName);
            eventParams.Add("eventTimestamp", DateTimeOffset.Now);
            eventParams.Add("eventData", new JArray());
            query.Add("events", new JArray(eventParams));

            // send query and get results
            Logging.Default.Information("Sending {EventName} event to Inara", eventName);
            string response = await _webClient.UploadStringTaskAsync("https://inara.cz/inapi/v1/", query.ToString());

            // return results
            IEnumerable<JToken> responseObjectsArray = JObject.Parse(response)["events"][0]?["eventData"]?.Children();
            if (responseObjectsArray == null)
                responseObjectsArray = new List<JToken>();
            Logging.Default.Debug("Retrieved {ObjectsCount} JSON event data objects from Inara", responseObjectsArray.Count());

            return responseObjectsArray
                .Select(cgJson => cgJson.ToObject<EliteCG>())
                // new: filter out old finished ones
                .Where(cg => !cg.IsCompleted || cg.ExpirationTime.Date >= Config.EliteAPI.MinDate);
            ;
        }
    }
}
