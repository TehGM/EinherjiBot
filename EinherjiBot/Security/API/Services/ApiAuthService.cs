namespace TehGM.EinherjiBot.Security.API.Services
{
    public class ApiAuthService : IAuthService
    {
        private readonly IDiscordAuthProvider _auth;
        private readonly IDiscordAuthHttpClient _client;
        private readonly IJwtGenerator _jwt;
        private readonly JwtOptions _options;

        public ApiAuthService(IDiscordAuthProvider auth, IDiscordAuthHttpClient client, IJwtGenerator jwtGenerator, IOptionsSnapshot<JwtOptions> options)
        {
            this._auth = auth;
            this._client = client;
            this._jwt = jwtGenerator;
            this._options = options.Value;
        }

        public async Task<LoginResponse> LoginAsync(string accessCode, CancellationToken cancellationToken = default)
        {
            DiscordAccessTokenResponse accessToken = await this._client.ExchangeCodeAsync(accessCode, cancellationToken).ConfigureAwait(false);
            CurrentUserResponse currentUser = await this._client.GetCurrentUserAsync(accessToken.AccessToken, cancellationToken).ConfigureAwait(false);
            UserSecurityData securityData = await this._auth.GetUserSecurityDataAsync(currentUser.ID, cancellationToken).ConfigureAwait(false);
            if (securityData.IsBanned)
                return null;

            string jwt = this._jwt.Generate(securityData);
            return new LoginResponse(jwt, this._options.Lifetime, currentUser, securityData.Roles);
        }
    }
}
