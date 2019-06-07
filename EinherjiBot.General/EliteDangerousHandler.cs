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
        private WebClient _webClient = new WebClient();
        private CancellationTokenSource _autoModeCTS;

        public EliteDangerousHandler(DiscordSocketClient client, BotConfig config) : base(client, config)
        {
            _webClient.Headers[HttpRequestHeader.UserAgent] = config.Auth.InaraAPI.AppName;
            _webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

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
                    TimeSpan timeLeft = DateTime.UtcNow - (Config.Data.EliteAPI.AutoNewsRetrievalTimeUtc + Config.EliteAPI.EliteAutoNewsInterval);
                    // if still waiting, await time, and repeat iteration
                    if (timeLeft > TimeSpan.Zero)
                    {
                        await Task.Delay(timeLeft, token);
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

        private async Task CmdCommunityGoals(SocketCommandContext message, Match match)
        {
            _cgCache = (await QueryForCGs()).ToList();
            for (int i = 0; i < _cgCache.Count; i++)
                await message.ReplyAsync(null, false, _cgCache[i].ToEmbed().Build());
        }

        private async Task<IEnumerable<EliteCG>> QueryForCGs()
        {
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
