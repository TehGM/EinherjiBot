using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.Settings
{
    public class GuildSettingsRequest : ICreateValidatable, IUpdateValidatable<IGuildSettings>, IValidatableObject
    {
        [JsonProperty("joinNotification")]
        public JoinLeaveSettingsRequest JoinNotification { get; set; }
        [JsonProperty("leaveNotification")]
        public JoinLeaveSettingsRequest LeaveNotification { get; set; }
        [JsonProperty("maxMessageTriggers")]
        public uint? MaxMessageTriggers { get; set; }

        [JsonConstructor]
        public GuildSettingsRequest() { }

        public GuildSettingsRequest(JoinLeaveSettingsRequest joinNotification, JoinLeaveSettingsRequest leaveNotification, uint? maxMessageTriggers)
        {
            this.JoinNotification = joinNotification;
            this.LeaveNotification = leaveNotification;
            this.MaxMessageTriggers = maxMessageTriggers;
        }

        public static GuildSettingsRequest FromSettings(IGuildSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            return new GuildSettingsRequest(
                JoinLeaveSettingsRequest.FromSettings(settings.JoinNotification),
                JoinLeaveSettingsRequest.FromSettings(settings.LeaveNotification),
                settings.MaxMessageTriggers);
        }

        public IEnumerable<string> ValidateForCreation()
        {
            if (this.JoinNotification != null)
            {
                foreach (string error in this.JoinNotification.ValidateForCreation())
                    yield return error;
            }
            if (this.LeaveNotification != null)
            {
                foreach (string error in this.LeaveNotification.ValidateForCreation())
                    yield return error;
            }
        }

        public IEnumerable<string> ValidateForUpdate(IGuildSettings existing)
            => this.ValidateForCreation();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            => this.ValidateForCreation().Select(e => new ValidationResult(e));
    }
}
