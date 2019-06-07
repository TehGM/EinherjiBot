using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
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

namespace TehGM.EinherjiBot
{
    class EliteDangerousHandler : HandlerBase
    {
        private IList<EliteCG> _cgCache = new List<EliteCG>();
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

            StartAutomaticNewsPosting();
        }

        private void StartAutomaticNewsPosting()
        {
            if (_autoModeCTS != null)
                return;
            _autoModeCTS = new CancellationTokenSource();
            Task autoTask = Task.Run(async () =>
            {
                CancellationToken token = _autoModeCTS.Token;
                while (!token.IsCancellationRequested)
                {
                    TimeSpan nextUpdateIn = (Config.Data.EliteAPI.AutoNewsRetrievalTimeUtc + Config.EliteAPI.EliteAutoNewsInterval) - DateTime.UtcNow;
                    // if still waiting, await time, and repeat iteration
                    if (nextUpdateIn >= TimeSpan.Zero)
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
                        EliteCG existingCG = _cgCache.FirstOrDefault(ecg => ecg.Equals(cg));
                        if (existingCG == null || existingCG.IsCompleted != cg.IsCompleted)
                            newOrJustFinishedCGs.Add(cg);
                    }
                    _cgCache = allCGs.ToList();
                    _cacheUpdateTimeUtc = DateTime.UtcNow;


                    // post all CGs
                    bool firstPost = true;
                    foreach (var cg in newOrJustFinishedCGs)
                    {
                        Embed cgEmbed = cg.ToEmbed().Build();

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
                // clear CTS on exiting if it wasn't cleared yet
                if (_autoModeCTS?.Token == token)
                    _autoModeCTS = null;
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
                await message.ReplyAsync("You will now get pinged or PMed when Elite Dangerous' Community Goals are updated. \u2705");
            }
            else
                await message.ReplyAsync("You already are subscribed to Elite Dangerous' Community Goals updates. \u274C");
        }

        private async Task CmdCommunityGoalsUnsubscribe(SocketCommandContext message, Match match)
        {
            if (Config.Data.EliteAPI.RemoveCommunityGoalsSubscriber(message.User))
            {
                await Config.Data.SaveAsync();
                await message.ReplyAsync("You'll no longer get pinged or PMed when Elite Dangerous' Community Goals are updated. \u2705");
            }
            else
                await message.ReplyAsync("You are not subscribed to Elite Dangerous' Community Goals updates. \u274C");
        }

        private async Task CmdCommunityGoals(SocketCommandContext message, Match match)
        {
            _cgCache = (await QueryForCGs()).ToList();
            _cacheUpdateTimeUtc = DateTime.UtcNow;
            for (int i = 0; i < _cgCache.Count; i++)
                await message.ReplyAsync(null, false, _cgCache[i].ToEmbed().Build());
        }

        private async Task<IEnumerable<EliteCG>> QueryForCGs()
        {
            // use cache if it's too early for retrieving again
            if ((DateTime.UtcNow - _cacheUpdateTimeUtc) < Config.EliteAPI.CachedCGLifetime)
                return _cgCache;

            // build query content
            JObject query = new JObject();
            query.Add("header", JToken.FromObject(Config.Auth.InaraAPI));
            JObject eventParams = new JObject();
            eventParams.Add("eventName", "getCommunityGoalsRecent");
            eventParams.Add("eventTimestamp", DateTimeOffset.Now);
            eventParams.Add("eventData", new JArray());
            query.Add("events", new JArray(eventParams));

            // send query and get results
            string response = await _webClient.UploadStringTaskAsync("https://inara.cz/inapi/v1/", query.ToString());

            // return results
            IEnumerable<JToken> responseObjectsArray = JObject.Parse(response)["events"][0]["eventData"].Children();
            return responseObjectsArray.Select(cgJson => cgJson.ToObject<EliteCG>());
        }
    }
}
