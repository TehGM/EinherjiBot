using Microsoft.AspNetCore.Components;
using System.Net.Http;
using TehGM.EinherjiBot.Security.API;

namespace TehGM.EinherjiBot.UI.Security.Services
{
    public class WebAuthService : IAuthService
    {
        private readonly HttpClient _client;

        public WebAuthService(HttpClient client, NavigationManager navigation)
        {
            this._client = client;
            this._client.BaseAddress = new Uri(navigation.BaseUri + "api/", UriKind.Absolute);
            this._client.DefaultRequestHeaders.Add("User-Agent", $"Einherji Web Client v{EinherjiInfo.WebVersion}");
        }

        public Task<LoginResponse> LoginAsync(string accessCode, CancellationToken cancellationToken = default)
        {
            LoginRequest request = new LoginRequest(accessCode);
            return this._client.PostJsonAsync<LoginResponse>("auth/token", request, cancellationToken);
        }

        public Task<LoginResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshRequest request = new RefreshRequest(refreshToken);
            return this._client.PostJsonAsync<LoginResponse>("auth/token/refresh", request, cancellationToken);
        }

        public Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshRequest request = new RefreshRequest(refreshToken);
            return this._client.DeleteJsonAsync("auth/token", request, cancellationToken);
        }
    }
}
