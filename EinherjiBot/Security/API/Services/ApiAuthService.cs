namespace TehGM.EinherjiBot.Security.API.Services
{
    public class ApiAuthService : IAuthService
    {
        private readonly IDiscordAuthProvider _auth;
        private readonly IDiscordAuthHttpClient _client;
        private readonly IJwtGenerator _jwt;
        private readonly IRefreshTokenStore _refreshTokens;
        private readonly JwtOptions _options;

        public ApiAuthService(IDiscordAuthProvider auth, IDiscordAuthHttpClient client, IJwtGenerator jwtGenerator, IRefreshTokenStore refreshTokens, IOptionsSnapshot<JwtOptions> options)
        {
            this._auth = auth;
            this._client = client;
            this._jwt = jwtGenerator;
            this._refreshTokens = refreshTokens;
            this._options = options.Value;
        }

        public async Task<LoginResponse> LoginAsync(string accessCode, CancellationToken cancellationToken = default)
        {
            DiscordAccessTokenResponse accessToken = await this._client.ExchangeCodeAsync(accessCode, cancellationToken).ConfigureAwait(false);
            return await this.AuthenticateAsync(accessToken, cancellationToken).ConfigureAwait(false);
        }

        public async Task<LoginResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshToken token = await this._refreshTokens.GetAsync(refreshToken, cancellationToken).ConfigureAwait(false);
            if (token == null)
                throw null;

            DiscordAccessTokenResponse accessToken = await this._client.RefreshAsync(token.DiscordRefreshToken, cancellationToken).ConfigureAwait(false);
            return await this.AuthenticateAsync(accessToken, cancellationToken).ConfigureAwait(false);
        }

        private async Task<LoginResponse> AuthenticateAsync(DiscordAccessTokenResponse accessToken, CancellationToken cancellationToken)
        {
            CurrentUserResponse currentUser = await this._client.GetCurrentUserAsync(accessToken.AccessToken, cancellationToken).ConfigureAwait(false);
            UserSecurityData securityData = await this._auth.GetUserSecurityDataAsync(currentUser.ID, cancellationToken).ConfigureAwait(false);
            if (securityData.IsBanned)
                return null;

            RefreshToken refreshToken = RefreshToken.Create(securityData.ID, accessToken.RefreshToken, TimeSpan.FromDays(5));
            await this._refreshTokens.AddAsync(refreshToken, cancellationToken).ConfigureAwait(false);

            string jwt = this._jwt.Generate(securityData);
            return new LoginResponse(jwt, refreshToken.Token, this._options.Lifetime, currentUser, securityData.Roles);
        }

        public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshToken token = await this._refreshTokens.GetAsync(refreshToken, cancellationToken).ConfigureAwait(false);
            if (token == null)
                return;

            await this._refreshTokens.DeleteAsync(token.Token, cancellationToken).ConfigureAwait(false);
            await this._client.RevokeAsync(token.DiscordRefreshToken, cancellationToken).ConfigureAwait(false);
        }
    }
}
