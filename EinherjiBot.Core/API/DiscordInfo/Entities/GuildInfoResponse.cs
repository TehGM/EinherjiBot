using Newtonsoft.Json;
using System.Diagnostics;

namespace TehGM.EinherjiBot.API
{
    [DebuggerDisplay("{ToString(),nq} ({ID,nq})")]
    public class GuildInfoResponse : IDiscordEntityInfo
    {
        [JsonProperty("id")]
        public ulong ID { get; init; }
        [JsonProperty("name")]
        public string Name { get; init; }
        [JsonProperty("users", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<GuildUserInfoResponse> Users { get; init; }
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<RoleInfoResponse> Roles { get; init; }

        [JsonConstructor]
        private GuildInfoResponse() { }

        public GuildInfoResponse(ulong id, string name)
        {
            this.ID = id;
            this.Name = name;
        }

        public ulong GetCacheKey()
            => this.ID;

        public override string ToString()
            => this.Name;

        public override int GetHashCode()
            => HashCode.Combine(this.ID);
    }
}
