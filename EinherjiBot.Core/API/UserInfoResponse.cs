using Newtonsoft.Json;

namespace TehGM.EinherjiBot.API
{
    public class UserInfoResponse
    {
        [JsonProperty("id")]
        public ulong ID { get; init; }
        [JsonProperty("username")]
        public string Username { get; init; }
        [JsonProperty("discriminator")]
        public string Discriminator { get; init; }
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Include)]
        public string AvatarHash { get; init; }

        [JsonConstructor]
        private UserInfoResponse() { }

        public UserInfoResponse(ulong id, string username, string discriminator, string avatarHash)
        {
            this.ID = id;
            this.Username = username;
            this.Discriminator = discriminator;
            this.AvatarHash = avatarHash;
        }

        public override string ToString()
            => $"{this.Username}#{this.Discriminator}";
    }
}
