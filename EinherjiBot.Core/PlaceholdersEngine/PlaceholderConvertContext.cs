using Newtonsoft.Json;

namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    public class PlaceholderConvertContext
    {
        [JsonProperty("type", Required = Required.Always)]
        public PlaceholderUsage ContextType { get; set; }

        [JsonProperty("messageContent", NullValueHandling = NullValueHandling.Ignore)]
        public string MessageContent { get; set; }
        [JsonProperty("guildId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ulong? CurrentGuildID { get; set; }
        [JsonProperty("channelId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ulong? CurrentChannelID { get; set; }
        [JsonProperty("userId", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public ulong? CurrentUserID { get; set; }

        public PlaceholderConvertContext(PlaceholderUsage contextType)
        {
            this.ContextType = contextType;
        }

        public static explicit operator PlaceholderConvertContext(PlaceholderUsage usage)
            => new PlaceholderConvertContext(usage);
        public static explicit operator PlaceholderUsage(PlaceholderConvertContext context)
            => context.ContextType;
    }
}
