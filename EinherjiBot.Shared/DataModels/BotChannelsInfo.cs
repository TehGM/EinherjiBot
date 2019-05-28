using Newtonsoft.Json;
using System.Collections.Generic;

namespace TehGM.EinherjiBot.DataModels
{
    public class BotChannelsInfo
    {
        [JsonProperty("akinatorId")]
        public ulong AkinatorID { get; private set; }
        [JsonProperty("radioinatorId")]
        public ulong RadioinatorID { get; private set; }
        [JsonProperty("rythmId")]
        public ulong RythmID { get; private set; }
        [JsonProperty("albionMarketId")]
        public ulong AlbionMarketID { get; private set; }

        [JsonProperty("musicChannelsIds")]
        public HashSet<ulong> MusicChannelsIDs { get; private set; }
        [JsonProperty("akinatorChannelsIds")]
        public HashSet<ulong> AkinatorChannelsIDs { get; private set; }
        [JsonProperty("otherBotsChannelsIds")]
        public HashSet<ulong> OtherBotsChannelsIDs { get; private set; }
    }
}
