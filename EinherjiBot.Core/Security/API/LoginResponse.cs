using Newtonsoft.Json;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.Security.API
{
    public class LoginResponse
    {
        [JsonProperty("token")]
        public string Token { get; init; }
        [JsonProperty("refreshToken")]
        public string RefreshToken { get; init; }
        [JsonProperty("tokenExpiration")]
        public uint TokenExpirationSeconds { get; init; }
        [JsonProperty("user")]
        public UserInfoResponse User { get; init; }
        [JsonProperty("roles")]
        public IEnumerable<string> Roles { get; init; }
        [JsonProperty("guilds", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<UserGuildInfoResponse> Guilds { get; init; }

        [JsonConstructor]
        private LoginResponse() { }

        public LoginResponse(string token, string refreshToken, TimeSpan expiration, UserInfoResponse user, IEnumerable<string> roles)
        {
            this.Token = token;
            this.RefreshToken = refreshToken;
            this.TokenExpirationSeconds = (uint)expiration.TotalSeconds;
            this.User = user;
            this.Roles = roles;
        }
    }
}
