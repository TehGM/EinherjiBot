using Newtonsoft.Json;

namespace TehGM.EinherjiBot.Settings
{
    public class GuildSettingsResponse : IGuildSettings, ICacheableEntity<ulong>
    {
        [JsonProperty("guildId")]
        public ulong GuildID { get; init; }
        [JsonProperty("joinNotification")]
        public JoinLeaveSettingsResponse JoinNotification { get; init; }
        [JsonProperty("leaveNotification")]
        public JoinLeaveSettingsResponse LeaveNotification { get; init; }
        [JsonProperty("maxMessageTriggers")]
        public uint? MaxMessageTriggers { get; init; }

        IJoinLeaveSettings IGuildSettings.JoinNotification => this.JoinNotification;
        IJoinLeaveSettings IGuildSettings.LeaveNotification => this.LeaveNotification;

        [JsonConstructor]
        private GuildSettingsResponse() { }

        public GuildSettingsResponse(ulong guildID, JoinLeaveSettingsResponse joinNotification, JoinLeaveSettingsResponse leaveNotification, uint? maxMessageTriggers)
        {
            this.GuildID = guildID;
            this.JoinNotification = joinNotification;
            this.LeaveNotification = leaveNotification;
            this.MaxMessageTriggers = maxMessageTriggers;
        }

        public ulong GetCacheKey()
            => this.GuildID;
    }
}
