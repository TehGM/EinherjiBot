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
        [JsonProperty("color")]
        public uint Color { get; init; }

        [JsonConstructor]
        private RoleInfoResponse() { }

        public RoleInfoResponse(ulong id, string name, uint color = 0)
        {
            this.ID = id;
            this.Name = name;
            this.Color = color;
        }

        public ulong GetCacheKey()
            => this.ID;
    }
}
