using System.Net.Http;
using TehGM.EinherjiBot.Security.API;
using TehGM.EinherjiBot.UI.Security;

namespace TehGM.EinherjiBot.UI.API.Services
{
    public class ApiRefreshTokenHttpHandler : DelegatingHandler
    {
        private readonly IAuthService _authService;
        private readonly IWebAuthProvider _authProvider;
        private readonly IRefreshTokenProvider _tokenProvider;

        public ApiRefreshTokenHttpHandler(IAuthService authService, IWebAuthProvider authProvider, IRefreshTokenProvider tokenProvider)
        {
            this._authService = authService;
            this._authProvider = authProvider;
            this._tokenProvider = tokenProvider;
        }

        protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            this.RefreshAsync(cancellationToken).GetAwaiter().GetResult();
            return base.Send(request, cancellationToken);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await this.RefreshAsync(cancellationToken).ConfigureAwait(false);
            return await base.SendAsync(request, cancellationToken);
        }

        private async ValueTask RefreshAsync(CancellationToken cancellationToken)
        {
            if (!this._authProvider.User.IsLoggedIn())
                return;
            if (DateTime.UtcNow < this._authProvider.Expiration.AddSeconds(-5))
                return;
            string token = await this._tokenProvider.GetAsync(cancellationToken).ConfigureAwait(false);
            LoginResponse response = await this._authService.RefreshAsync(token, cancellationToken).ConfigureAwait(false);
            await this._authProvider.LoginAsync(response, cancellationToken).ConfigureAwait(false);
        }
    }
}
