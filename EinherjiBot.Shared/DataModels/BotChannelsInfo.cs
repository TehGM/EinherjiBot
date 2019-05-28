using Newtonsoft.Json;
using System.Collections.Generic;

namespace TehGM.EinherjiBot.DataModels
{
    public class BotChannelsInfo
    {
        // bots ids
        [JsonProperty("akinatorId")]
        public ulong AkinatorID { get; private set; }
        [JsonProperty("radioinatorId")]
        public ulong RadioinatorID { get; private set; }
        [JsonProperty("rythmId")]
        public ulong RythmID { get; private set; }
        [JsonProperty("albionMarketId")]
        public ulong AlbionMarketID { get; private set; }

        // channels ids
        [JsonProperty("musicChannelsIds")]
        public HashSet<ulong> MusicChannelsIDs { get; private set; }
        [JsonProperty("akinatorChannelsIds")]
        public HashSet<ulong> AkinatorChannelsIDs { get; private set; }
        [JsonProperty("otherBotsChannelsIds")]
        public HashSet<ulong> OtherBotsChannelsIDs { get; private set; }

        // special channels ids
        [JsonProperty("ignoreChannelsIds")]
        public HashSet<ulong> IgnoreChannelsIDs { get; private set; }
        [JsonProperty("ignoreUsersIds")]
        public HashSet<ulong> IgnoreUsersIDs { get; private set; }
    }
}
