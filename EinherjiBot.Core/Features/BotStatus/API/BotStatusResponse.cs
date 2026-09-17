using Newtonsoft.Json;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.BotStatus
{
    public class BotStatusResponse : IBotStatus
    {
        [JsonProperty("id")]
        public Guid ID { get; init; }
        [JsonProperty("text")]
        public string Text { get; init; }
        [JsonProperty("enabled")]
        public bool IsEnabled { get; init; }
        [JsonProperty("lastError", NullValueHandling = NullValueHandling.Ignore)]
        public ErrorInfoResponse LastError { get; init; }

        IErrorInfo IBotStatus.LastError => this.LastError;

        public BotStatusResponse(Guid id, string text, bool isEnabled)
        {
            this.ID = id;
            this.Text = text;
            this.IsEnabled = isEnabled;
        }
    }
}
