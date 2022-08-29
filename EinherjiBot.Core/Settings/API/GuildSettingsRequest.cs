using Newtonsoft.Json;

namespace TehGM.EinherjiBot.Settings
{
    public class GuildSettingsRequest
    {
        [JsonProperty("joinNotificationChannel")]
        public ulong? JoinNotificationChannelID { get; set; }
        [JsonProperty("leaveNotificationChannel")]
        public ulong? LeaveNotificationChannelID { get; set; }
        [JsonProperty("maxMessageTriggers")]
        public uint? MaxMessageTriggers { get; set; }

        [JsonConstructor]
        public GuildSettingsRequest() { }

        public GuildSettingsRequest(ulong? joinNotificationChannelID, ulong? leaveNotificationChannelID, uint? maxMessageTriggers)
        {
            this.JoinNotificationChannelID = joinNotificationChannelID;
            this.LeaveNotificationChannelID = leaveNotificationChannelID;
            this.MaxMessageTriggers = maxMessageTriggers;
        }

        public static GuildSettingsRequest FromSettings(IGuildSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            return new GuildSettingsRequest(settings.JoinNotificationChannelID, settings.LeaveNotificationChannelID, settings.MaxMessageTriggers);
        }
    }
}
