using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.Security.API.Services
{
    public class ApiAuthService : IAuthService
    {
        private readonly IDiscordAuthProvider _auth;
        private readonly IDiscordAuthHttpClient _client;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IRefreshTokenGenerator _refreshTokenGenerator;
        private readonly IRefreshTokenStore _refreshTokenStore;
        private readonly IUserFeatureProvider _userFeatures;
        private readonly JwtOptions _options;

        public ApiAuthService(IDiscordAuthProvider auth, IDiscordAuthHttpClient client, IJwtGenerator jwtGenerator, IRefreshTokenGenerator refreshTokenGenerator,
            IRefreshTokenStore refreshTokenStore, IUserFeatureProvider userFeatures, IOptionsSnapshot<JwtOptions> options)
        {
            this._auth = auth;
            this._client = client;
            this._jwtGenerator = jwtGenerator;
            this._refreshTokenGenerator = refreshTokenGenerator;
            this._refreshTokenStore = refreshTokenStore;
            this._userFeatures = userFeatures;
            this._options = options.Value;
        }

        public async Task<LoginResponse> LoginAsync(string accessCode, CancellationToken cancellationToken = default)
        {
            DiscordAccessTokenResponse accessToken = await this._client.ExchangeCodeAsync(accessCode, cancellationToken).ConfigureAwait(false);
            return await this.AuthenticateAsync(accessToken, cancellationToken).ConfigureAwait(false);
        }

        public async Task<LoginResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshToken token = await this._refreshTokenStore.GetAsync(refreshToken, cancellationToken).ConfigureAwait(false);
            if (token == null)
                throw null;

            DiscordAccessTokenResponse accessToken = await this._client.RefreshAsync(token.DiscordRefreshToken, cancellationToken).ConfigureAwait(false);
            return await this.AuthenticateAsync(accessToken, cancellationToken).ConfigureAwait(false);
        }

        private async Task<LoginResponse> AuthenticateAsync(DiscordAccessTokenResponse accessToken, CancellationToken cancellationToken)
        {
            UserInfoResponse currentUser = await this._client.GetCurrentUserAsync(accessToken.AccessToken, cancellationToken).ConfigureAwait(false);
            IDiscordAuthContext context = await this._auth.GetAsync(currentUser.ID, null, cancellationToken).ConfigureAwait(false);
            if (context.IsBanned)
                return null;

            Task<IEnumerable<string>> featuresTask = this._userFeatures.GetForUserAsync(context, cancellationToken);
            Task<IEnumerable<OAuthGuildInfoResponse>> guildsTask = this._client.GetCurrentUserGuildsAsync(accessToken.AccessToken, cancellationToken);
            RefreshToken refreshToken = this._refreshTokenGenerator.Generate(context.ID, accessToken.RefreshToken);
            Task persistTokenTask = this._refreshTokenStore.AddAsync(refreshToken, cancellationToken);

            string jwt = this._jwtGenerator.Generate(context);
            await Task.WhenAll(guildsTask, persistTokenTask, featuresTask).ConfigureAwait(false);
            return new LoginResponse(jwt, refreshToken.Token, this._options.Lifetime, currentUser, context.BotRoles, featuresTask.Result)
            {
                Guilds = guildsTask.Result,
                KnownDiscordGuildIDs = context.KnownDiscordGuildIDs,
                KnownDiscordRoleIDs = context.KnownDiscordRoleIDs
            };
        }

        public async Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshToken token = await this._refreshTokenStore.GetAsync(refreshToken, cancellationToken).ConfigureAwait(false);
            if (token == null)
                return;

            await this._refreshTokenStore.DeleteAsync(token.Token, cancellationToken).ConfigureAwait(false);
            await this._client.RevokeAsync(token.DiscordRefreshToken, cancellationToken).ConfigureAwait(false);
        }
    }
}
