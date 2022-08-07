using Newtonsoft.Json;

namespace TehGM.EinherjiBot.API
{
    public class RoleInfoResponse : ICacheableEntity<ulong>
    {
        public static RoleInfoResponse None { get; } = new RoleInfoResponse();

        [JsonProperty("id")]
        public ulong ID { get; init; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("guild")]
        public ulong GuildID { get; init; }
        [JsonProperty("guildName")]
        public string GuildName { get; init; }
        [JsonProperty("color")]
        public uint Color { get; init; }

        [JsonConstructor]
        private RoleInfoResponse() { }

        public RoleInfoResponse(ulong id, string name, ulong guildID, string guildName, uint color = 0)
        {
            this.ID = id;
            this.Name = name;
            this.GuildID = guildID;
            this.GuildName = guildName;
            this.Color = color;
        }

        public ulong GetCacheKey()
            => this.ID;
    }
}
