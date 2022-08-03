using Microsoft.AspNetCore.Components;
using System.Net.Http;
using System.Net.Http.Headers;
using TehGM.EinherjiBot.Security.API;
using TehGM.EinherjiBot.UI.Security;

namespace TehGM.EinherjiBot.UI.API.Services
{
    public class ApiHttpClient : IApiClient
    {
        private readonly HttpClient _client;
        private readonly IWebAuthProvider _authProvider;
        private readonly IRefreshTokenProvider _refreshTokenProvider;

        public ApiHttpClient(HttpClient client, NavigationManager navigation, IWebAuthProvider authProvider, IRefreshTokenProvider refreshTokenProvider)
        {
            this._client = client;
            this._authProvider = authProvider;
            this._refreshTokenProvider = refreshTokenProvider;
            this._client.BaseAddress = new Uri(navigation.BaseUri + "api/", UriKind.Absolute);
            this._client.DefaultRequestHeaders.Add("User-Agent", $"Einherji Web Client v{EinherjiInfo.BotVersion}");
        }

        public async Task<TResponse> SendJsonAsync<TResponse>(HttpRequestMessage request, object data, CancellationToken cancellationToken = default)
        {
            await this.AttachTokenAsync(request, cancellationToken).ConfigureAwait(false);
            return await this._client.SendJsonAsync<TResponse>(request, data, "application/json", cancellationToken).ConfigureAwait(false);
        }

        private async Task AttachTokenAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await this.RefreshAsync(cancellationToken).ConfigureAwait(false);
            if (this._authProvider.User.IsLoggedIn())
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", this._authProvider.Token);
        }

        private async ValueTask RefreshAsync(CancellationToken cancellationToken)
        {
            if (!this._authProvider.User.IsLoggedIn())
                return;
            if (DateTime.UtcNow < this._authProvider.Expiration.AddSeconds(-5))
                return;
            string token = await this._refreshTokenProvider.GetAsync(cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrWhiteSpace(token))
            {
                LoginResponse response = await (this as IAuthService).RefreshAsync(token, cancellationToken).ConfigureAwait(false);
                await this._authProvider.LoginAsync(response, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
