using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        public EliteDangerousHandler(DiscordSocketClient client, BotConfig config) : base(client, config)
        {
            _webClient.Headers[HttpRequestHeader.UserAgent] = config.Auth.InaraAPI.AppName;
            _webClient.Headers[HttpRequestHeader.ContentType] = "application/json";

            CommandsStack.Add(new RegexUserCommand("^elite (?:cgs?|community goals?)", CmdCommunityGoals));
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
