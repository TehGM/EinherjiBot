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
        [JsonProperty("guildID")]
        public ulong GuildID { get; init; }
        [JsonProperty("bot")]
        public bool IsBot { get; init; }
        [JsonProperty("admin")]
        public bool IsAdmin { get; init; }
        [JsonProperty("owner")]
        public bool IsOwner { get; init; }

        [JsonConstructor]
        private GuildUserInfoResponse() : base() { }

        private RoleInfoResponse _topRoleWithColor;

        public GuildUserInfoResponse(ulong id, string username, string discriminator, string avatarHash, ulong guildID, IEnumerable<RoleInfoResponse> roles)
            : base(id, username, discriminator, avatarHash)
        {
            this.GuildID = guildID;
            this.Roles = roles;
        }

        public string GetGuildAvatarURL(ushort size = 1024)
        {
            if (string.IsNullOrWhiteSpace(this.GuildAvatarHash))
                return this.GetAvatarURL(size);

            string ext = this.AvatarHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png";
            return $"https://cdn.discordapp.com/guilds/{this.GuildID}/users/{this.ID}/avatars/{this.AvatarHash}.{ext}?size={size}";
        }

        public RoleInfoResponse GetTopRoleWithColor()
        {
            if (this._topRoleWithColor == null)
                this._topRoleWithColor = this.Roles.Where(r => r.Color != 0).MaxBy(r => r.Position);
            return this._topRoleWithColor;
        }
    }
}
