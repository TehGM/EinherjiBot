using Discord;
using Newtonsoft.Json;

namespace TehGM.EinherjiBot.Settings
{
    public class JoinLeaveSettingsResponse : IJoinLeaveSettings
    {
        [JsonProperty("enabled")]
        public bool IsEnabled { get; init; }
        [JsonProperty("useSystemChannel")]
        public bool UseSystemChannel { get; init; }
        [JsonProperty("channel")]
        public ulong? NotificationChannelID { get; init; }
        [JsonProperty("message")]
        public string MessageTemplate { get; init; }
        [JsonProperty("showAvatar")]
        public bool ShowUserAvatar { get; init; }
        [JsonProperty("color")]
        public uint EmbedColor { get; init; }

        [JsonIgnore]
        Color IJoinLeaveSettings.EmbedColor => this.EmbedColor;

        [JsonConstructor]
        private JoinLeaveSettingsResponse() { }

        public JoinLeaveSettingsResponse(bool isEnabled, bool useSystemChannel, ulong? notificationChannelID, string messageTemplate, bool showUserAvatar, Color embedColor)
        {
            this.IsEnabled = isEnabled;
            this.UseSystemChannel = useSystemChannel;
            this.NotificationChannelID = notificationChannelID;
            this.MessageTemplate = messageTemplate;
            this.ShowUserAvatar = showUserAvatar;
            this.EmbedColor = embedColor;
        }
    }
}
