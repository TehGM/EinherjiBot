using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Security.Claims;
using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.API;
using TehGM.EinherjiBot.UI.API;

namespace TehGM.EinherjiBot.UI.Security.Services
{
    public class WebAuthenticationStateProvider : AuthenticationStateProvider, IWebAuthProvider, IAuthProvider
    {
        public DateTime Expiration { get; private set; }
        public IAuthContext User { get; private set; } = WebAuthContext.None;
        public IEnumerable<UserGuildInfoResponse> Guilds { get; private set; }
        public string Token { get; private set; }
        public IEnumerable<string> UserFeatures { get; private set; }

        private ClaimsPrincipal _principal;
        private bool _loaded = false;

        private readonly IAuthService _authService;
        private readonly IRefreshTokenProvider _tokenProvider;

        public WebAuthenticationStateProvider(IAuthService authService, IRefreshTokenProvider tokenProvider)
        {
            this._authService = authService;
            this._tokenProvider = tokenProvider;
        }

        public async Task LoginAsync(LoginResponse response, CancellationToken cancellationToken = default)
        {
            this.User = WebAuthContext.FromLoginResponse(response);
            this.Token = response.Token;
            this.Expiration = DateTime.UtcNow.AddSeconds(response.TokenExpirationSeconds);
            this.Guilds = response.Guilds;
            this.UserFeatures = response.Features;
            this._principal = this.User.ToClaimsPrincipal("jwt");
            await this._tokenProvider.SetAsync(response.RefreshToken, cancellationToken).ConfigureAwait(false);
            base.NotifyAuthenticationStateChanged(Task.FromResult(this.GetState()));
        }

        public Task LogoutAsync(CancellationToken cancellationToken = default)
            => this.LogoutInternalAsync(true, cancellationToken);

        private async Task LogoutInternalAsync(bool removeRemoteToken, CancellationToken cancellationToken = default)
        {
            this.User = WebAuthContext.None;
            this.Token = null;
            this.Expiration = DateTime.UtcNow;
            this.Guilds = null;
            this.UserFeatures = null;
            this._principal = new ClaimsPrincipal();
            string token = await this._tokenProvider.GetAsync(cancellationToken).ConfigureAwait(false);
            if (token != null)
            {
                await this._tokenProvider.ClearAsync(cancellationToken).ConfigureAwait(false);
                if (removeRemoteToken)
                    await this._authService.LogoutAsync(token, cancellationToken).ConfigureAwait(false);
            }
            base.NotifyAuthenticationStateChanged(Task.FromResult(this.GetState()));
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (!this._loaded)
            {
                this._loaded = true;
                try
                {
                    string token = await this._tokenProvider.GetAsync().ConfigureAwait(false);
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        LoginResponse response = await this._authService.RefreshAsync(token).ConfigureAwait(false);
                        await this.LoginAsync(response).ConfigureAwait(false);
                    }
                }
                catch (Exception ex) when (ex is not ClientVersionException)
                {
                    await this.LogoutInternalAsync(false).ConfigureAwait(false);
                }
            }
            return this.GetState();
        }

        private AuthenticationState GetState()
        {
            if (this.User.IsLoggedIn())
                return new AuthenticationState(this._principal);
            else
                return new AuthenticationState(new ClaimsPrincipal());
        }
    }
}
