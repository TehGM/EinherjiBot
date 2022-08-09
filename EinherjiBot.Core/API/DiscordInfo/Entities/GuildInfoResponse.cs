using Newtonsoft.Json;
using System.Diagnostics;

namespace TehGM.EinherjiBot.API
{
    [DebuggerDisplay("{ToString(),nq} ({ID,nq})")]
    public class GuildInfoResponse : IDiscordGuildInfo, IDiscordEntityInfo
    {
        [JsonProperty("id")]
        public ulong ID { get; init; }
        [JsonProperty("name")]
        public string Name { get; init; }
        [JsonProperty("icon")]
        public string IconHash { get; init; }
        [JsonProperty("owner")]
        public ulong OwnerID { get; init; }
        [JsonProperty("users", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<GuildUserInfoResponse> Users { get; init; }
        [JsonProperty("roles", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<RoleInfoResponse> Roles { get; init; }

        [JsonConstructor]
        private GuildInfoResponse() { }

        public GuildInfoResponse(ulong id, string name, string iconHash)
        {
            this.ID = id;
            this.Name = name;
            this.IconHash = iconHash;
        }

        public ulong GetCacheKey()
            => this.ID;

        public override string ToString()
            => this.Name;

        public override int GetHashCode()
            => HashCode.Combine(this.ID);
    }
}
