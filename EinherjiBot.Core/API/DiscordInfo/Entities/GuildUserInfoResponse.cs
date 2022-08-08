using Newtonsoft.Json;
using System.Diagnostics;

namespace TehGM.EinherjiBot.API
{
    [DebuggerDisplay("{ToString(),nq} ({ID,nq})")]
    public class GuildUserInfoResponse : UserInfoResponse, IDiscordUserInfo
    {
        [JsonProperty("nickname")]
        public string Nickname { get; init; }
        [JsonProperty("roles")]
        public IEnumerable<RoleInfoResponse> Roles { get; init; }
        [JsonProperty("guildAvatarHash")]
        public string GuildAvatarHash { get; init; }

        [JsonConstructor]
        private GuildUserInfoResponse() : base() { }

        public GuildUserInfoResponse(ulong id, string username, string discriminator, string avatarHash, IEnumerable<RoleInfoResponse> roles)
            : base(id, username, discriminator, avatarHash)
        {
            this.Roles = roles;
        }
    }
}
