using Newtonsoft.Json;

namespace TehGM.EinherjiBot.Settings
{
    public class GuildSettingsResponse : IGuildSettings, ICacheableEntity<ulong>
    {
        [JsonProperty("guildId")]
        public ulong GuildID { get; init; }
        [JsonProperty("joinNotificationChannel")]
        public ulong? JoinNotificationChannelID { get; init; }
        [JsonProperty("leaveNotificationChannel")]
        public ulong? LeaveNotificationChannelID { get; init; }
        [JsonProperty("maxMessageTriggers")]
        public uint? MaxMessageTriggers { get; init; }

        [JsonConstructor]
        private GuildSettingsResponse() { }

        public GuildSettingsResponse(ulong guildID, ulong? joinNotificationChannelID, ulong? leaveNotificationChannelID, uint? maxMessageTriggers)
        {
            this.GuildID = guildID;
            this.JoinNotificationChannelID = joinNotificationChannelID;
            this.LeaveNotificationChannelID = leaveNotificationChannelID;
            this.MaxMessageTriggers = maxMessageTriggers;
        }

        public ulong GetCacheKey()
            => this.GuildID;
    }
}
