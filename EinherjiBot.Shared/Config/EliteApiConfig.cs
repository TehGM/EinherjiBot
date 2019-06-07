using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TehGM.EinherjiBot.Config
{
    public class EliteApiConfig
    {
        [JsonProperty("eliteAutoNewsChannelId")]
        public ulong EliteAutoNewsChannelID { get; private set; }
        [JsonProperty("eliteAutoNewsIntervalSeconds")]
        private uint _eliteAutoNewsIntervalSeconds;
        [JsonProperty("preferPingOverPm")]
        public bool PreferPingOverPM { get; private set; }
        [JsonIgnore]
        public TimeSpan EliteAutoNewsInterval
        {
            get { return TimeSpan.FromSeconds(_eliteAutoNewsIntervalSeconds); }
            set { _eliteAutoNewsIntervalSeconds = (uint)value.TotalSeconds; }
        }
    }
}
