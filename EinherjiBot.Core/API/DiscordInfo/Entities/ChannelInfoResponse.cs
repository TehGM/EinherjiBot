using Discord;
using Newtonsoft.Json;

namespace TehGM.EinherjiBot.API
{
    public class ChannelInfoResponse : IDiscordEntityInfo, ICacheableEntity<ulong>
    {
        [JsonProperty("id")]
        public ulong ID { get; init; }
        [JsonProperty("name")]
        public string Name { get; init; }
        [JsonProperty("type")]
        public ChannelType Type { get; init; }
        [JsonProperty("guild")]
        public ulong? GuildID { get; init; }
        [JsonProperty("guildName")]
        public string GuildName { get; init; }
        [JsonProperty("category")]
        public ulong? CategoryID { get; init; }
        [JsonProperty("topic")]
        public string Topic { get; init; }
        [JsonProperty("position")]
        public int Position { get; init; }

        [JsonConstructor]
        private ChannelInfoResponse() { }

        public ChannelInfoResponse(ulong id, string name, ChannelType type, ulong? guildID)
        {
            this.ID = id;
            this.Name = name;
            this.Type = type;
            this.GuildID = guildID;
        }

        public ulong GetCacheKey()
            => this.ID;
    }
}
