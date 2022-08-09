﻿using Newtonsoft.Json;
using System.Diagnostics;

namespace TehGM.EinherjiBot.API
{
    [DebuggerDisplay("{ToString(),nq} ({ID,nq})")]
    public class OAuthGuildInfoResponse : IDiscordGuildInfo, IDiscordEntityInfo
    {
        [JsonProperty("id")]
        public ulong ID { get; init; }
        [JsonProperty("name")]
        public string Name { get; init; }
        [JsonProperty("icon")]
        public string IconHash { get; init; }
        [JsonProperty("owner")]
        public bool IsOwner { get; init; }
        [JsonProperty("permissions")]
        public ulong PermissionFlags { get; init; }
        [JsonProperty("features")]
        public IEnumerable<string> EnabledFeatures { get; init; }

        [JsonIgnore]
        public bool CanManage
            => this.IsOwner
            || (this.PermissionFlags & 0x8uL) == 0x8uL;  // admin

        [JsonConstructor]
        private OAuthGuildInfoResponse() { }

        public override string ToString()
            => this.Name;

        public ulong GetCacheKey()
            => this.ID;

        public override int GetHashCode()
            => HashCode.Combine(this.ID);
    }
}
