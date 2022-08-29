using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.Settings
{
    public class JoinLeaveSettingsRequest : ICreateValidatable, IUpdateValidatable<IJoinLeaveSettings>, IValidatableObject
    {
        [JsonProperty("enabled")]
        public bool IsEnabled { get; set; }
        [JsonProperty("useSystemChannel")]
        public bool UseSystemChannel { get; set; }
        [JsonProperty("channel")]
        public ulong? NotificationChannelID { get; set; }
        [JsonProperty("message")]
        public string MessageTemplate { get; set; }
        [JsonProperty("showAvatar")]
        public bool ShowUserAvatar { get; set; }
        [JsonProperty("color")]
        public uint EmbedColor { get; set; }

        [JsonConstructor]
        public JoinLeaveSettingsRequest() { }

        public JoinLeaveSettingsRequest(bool isEnabled, bool useSystemChannel, ulong? notificationChannelID, string messageTemplate, bool showUserAvatar, uint embedColor)
        {
            this.IsEnabled = isEnabled;
            this.UseSystemChannel = useSystemChannel;
            this.NotificationChannelID = notificationChannelID;
            this.MessageTemplate = messageTemplate;
            this.ShowUserAvatar = showUserAvatar;
            this.EmbedColor = embedColor;
        }

        public static JoinLeaveSettingsRequest FromSettings(IJoinLeaveSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            return new JoinLeaveSettingsRequest(settings.IsEnabled, settings.UseSystemChannel, settings.NotificationChannelID, settings.MessageTemplate, settings.ShowUserAvatar, settings.EmbedColor);
        }

        public IEnumerable<string> ValidateForCreation()
        {
            if (this.IsEnabled)
            {
                if (!this.UseSystemChannel && this.NotificationChannelID == null)
                    yield return "Channel for notification must be specified.";
                if (string.IsNullOrWhiteSpace(this.MessageTemplate))
                    yield return "Message template cannot be empty.";
            }
        }

        public IEnumerable<string> ValidateForUpdate(IJoinLeaveSettings existing)
            => this.ValidateForCreation();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            => this.ValidateForCreation().Select(e => new ValidationResult(e));
    }
}
