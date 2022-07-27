using Newtonsoft.Json;

namespace TehGM.EinherjiBot.Security.API
{
    public class DiscordAccessTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; init; }
        [JsonProperty("token_type")]
        public string TokenType { get; init; }
        [JsonProperty("expires_in")]
        public uint ExpirationSeconds { get; init; }
        [JsonProperty("refresh_token")]
        public string RefreshToken { get; init; }
        [JsonProperty("scope")]
        public string Scope { get; init; }

        [JsonConstructor]
        private DiscordAccessTokenResponse() { }
    }
}
