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
        [JsonProperty("chachedCgLifetimeSeconds")]
        private uint _chachedCgLifetimeSeconds;
        [JsonProperty("preferPingOverPm")]
        public bool PreferPingOverPM { get; private set; }
        [JsonProperty("thumbnailUrl")]
        public string ThumbnailURL { get; private set; }
        [JsonProperty("maxAge")]
        public int MaxAge { get; private set; }
        [JsonIgnore]
        public TimeSpan EliteAutoNewsInterval
        {
            get { return TimeSpan.FromSeconds(_eliteAutoNewsIntervalSeconds); }
            set { _eliteAutoNewsIntervalSeconds = (uint)value.TotalSeconds; }
        }
        [JsonIgnore]
        public TimeSpan CachedCGLifetime
        {
            get { return TimeSpan.FromSeconds(_chachedCgLifetimeSeconds); }
            set { _chachedCgLifetimeSeconds = (uint)value.TotalSeconds; }
        }
        [JsonIgnore]
        public DateTimeOffset MinDate
        {
            get { return DateTimeOffset.Now.Date.AddDays(-MaxAge); }
            set { this.MaxAge = (int)(DateTimeOffset.Now.Date - value).TotalDays; }
        }
    }
}
