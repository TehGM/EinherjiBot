using Newtonsoft.Json;
using System.Diagnostics;

namespace TehGM.EinherjiBot.API
{
    [DebuggerDisplay("{ToString(),nq} ({ID,nq})")]
    public class UserInfoResponse : IDiscordUserInfo, ICacheableEntity<ulong>
    {
        [JsonProperty("id")]
        public ulong ID { get; init; }
        [JsonProperty("username")]
        public string Username { get; init; }
        [JsonProperty("discriminator")]
        public string Discriminator { get; init; }
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Include)]
        public string AvatarHash { get; init; }

        [JsonIgnore]
        string IDiscordEntityInfo.Name => this.Username;

        [JsonConstructor]
        protected UserInfoResponse() { }

        public UserInfoResponse(ulong id, string username, string discriminator, string avatarHash)
        {
            this.ID = id;
            this.Username = username;
            this.Discriminator = discriminator;
            this.AvatarHash = avatarHash;
        }

        public override string ToString()
            => this.GetUsernameWithDiscriminator();

        public override int GetHashCode()
            => HashCode.Combine(this.ID);

        public ulong GetCacheKey()
            => this.ID;
    }
}
