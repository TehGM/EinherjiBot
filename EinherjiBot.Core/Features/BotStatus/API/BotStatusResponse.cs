using Discord;
using Newtonsoft.Json;

namespace TehGM.EinherjiBot.BotStatus.API
{
    public class BotStatusResponse
    {
        [JsonProperty("id")]
        public Guid ID { get; init; }
        [JsonProperty("text")]
        public string Text { get; init; }
        [JsonProperty("link")]
        public string Link { get; init; }
        [JsonProperty("activity")]
        public ActivityType ActivityType { get; init; }
        [JsonProperty("enabled")]
        public bool IsEnabled { get; init; }

        public BotStatusResponse(Guid id, string text, string link, ActivityType activityType, bool isEnabled)
        {
            this.ID = id;
            this.Text = text;
            this.Link = link;
            this.ActivityType = activityType;
            this.IsEnabled = isEnabled;
        }
    }
}
