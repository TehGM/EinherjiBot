using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.API;

namespace TehGM.EinherjiBot.UI.Security.Services
{
    public class WebAuthenticationStateProvider : AuthenticationStateProvider, IWebAuthProvider, IAuthProvider
    {
        public DateTime Expiration { get; private set; }
        public IAuthContext User { get; private set; } = WebAuthContext.None;
        public string Token { get; private set; }
        private ClaimsPrincipal _principal;
        private bool _loaded = false;

        public bool IsLoggedIn => this.User != null && !this.User.Equals(WebAuthContext.None);

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
            this._principal = this.User.ToClaimsPrincipal("jwt");
            await this._tokenProvider.SetAsync(response.RefreshToken, cancellationToken).ConfigureAwait(false);
            base.NotifyAuthenticationStateChanged(Task.FromResult(this.GetState()));
        }

        public async Task LogoutAsync(CancellationToken cancellationToken = default)
        {
            this.User = WebAuthContext.None;
            this.Token = null;
            this.Expiration = DateTime.UtcNow;
            this._principal = new ClaimsPrincipal();
            string token = await this._tokenProvider.GetAsync(cancellationToken).ConfigureAwait(false);
            if (token != null)
            {
                await this._tokenProvider.SetAsync(null, cancellationToken).ConfigureAwait(false);
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
                    LoginResponse response = await this._authService.RefreshAsync(token).ConfigureAwait(false);
                    await this.LoginAsync(response).ConfigureAwait(false);
                }
                catch
                {
                    await this.LogoutAsync().ConfigureAwait(false);
                }
            }
            return this.GetState();
        }

        private AuthenticationState GetState()
        {
            if (this.IsLoggedIn)
                return new AuthenticationState(this._principal);
            else
                return new AuthenticationState(new ClaimsPrincipal());
        }
    }
}
