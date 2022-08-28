using Discord;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Utilities;

namespace TehGM.EinherjiBot.BotStatus
{
    public class BotStatusRequest : ICreateValidatable, IUpdateValidatable<IBotStatus>, IValidatableObject
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("link")]
        public string Link { get; set; }
        [JsonProperty("activity")]
        public ActivityType ActivityType { get; set; }
        [JsonProperty("enabled")]
        public bool IsEnabled { get; set; }

        [JsonConstructor]
        public BotStatusRequest() { }

        public BotStatusRequest(string text, string link, ActivityType activityType, bool isEnabled)
        {
            this.Text = text;
            this.Link = link;
            this.ActivityType = activityType;
            this.IsEnabled = isEnabled;
        }

        public IEnumerable<string> ValidateForCreation()
            => this.ValidateShared();

        public IEnumerable<string> ValidateForUpdate(IBotStatus existing)
            => this.ValidateShared();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            => this.ValidateShared().Select(e => new ValidationResult(e));

        private IEnumerable<string> ValidateShared()
        {
            if (this.ActivityType == ActivityType.Streaming)
            {
                foreach (string error in ActivityLinkValidator.ValidationDelegate(this.Link))
                    yield return error;
            }
            else if (!string.IsNullOrEmpty(this.Link))
                yield return $"Link is only valid for {ActivityType.Streaming} activity type.";
        }

        public static BotStatusRequest FromStatus(IBotStatus status)
            => new BotStatusRequest(status.Text, status.Link, status.ActivityType, status.IsEnabled);
    }
}
