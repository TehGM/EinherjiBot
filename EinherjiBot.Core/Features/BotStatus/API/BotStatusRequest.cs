using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.BotStatus
{
    public class BotStatusRequest : ICreateValidatable, IUpdateValidatable<IBotStatus>, IValidatableObject
    {
        [JsonProperty("text")]
        public string Text { get; set; }
        [JsonProperty("enabled")]
        public bool IsEnabled { get; set; }

        [JsonConstructor]
        public BotStatusRequest() { }

        public BotStatusRequest(string text, bool isEnabled)
        {
            this.Text = text;
            this.IsEnabled = isEnabled;
        }

        public IEnumerable<string> ValidateForCreation()
            => this.ValidateShared();

        public IEnumerable<string> ValidateForUpdate(IBotStatus existing)
            => this.ValidateShared();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            => this.ValidateShared().Select(e => new ValidationResult(e));

        private IEnumerable<string> ValidateShared()
            => Enumerable.Empty<string>();

        public static BotStatusRequest FromStatus(IBotStatus status)
            => new BotStatusRequest(status.Text, status.IsEnabled);
    }
}
