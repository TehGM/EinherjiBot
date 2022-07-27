﻿using Newtonsoft.Json;

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
        public CurrentUserResponse User { get; init; }
        [JsonProperty("roles")]
        public IEnumerable<string> Roles { get; init; }

        [JsonConstructor]
        private LoginResponse() { }

        public LoginResponse(string token, string refreshToken, TimeSpan expiration, CurrentUserResponse user, IEnumerable<string> roles)
        {
            this.Token = token;
            this.RefreshToken = refreshToken;
            this.TokenExpirationSeconds = (uint)expiration.TotalSeconds;
            this.User = user;
            this.Roles = roles;
        }
    }
}
