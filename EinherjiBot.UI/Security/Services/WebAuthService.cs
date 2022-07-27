using Microsoft.AspNetCore.Components;
using System.Net.Http;
using TehGM.EinherjiBot.Security.API;
using TehGM.EinherjiBot.UI.API.Services;

namespace TehGM.EinherjiBot.UI.Security.Services
{
    public class WebAuthService : ApiHttpClient, IAuthService
    {
        public WebAuthService(HttpClient client, NavigationManager navigation)
            : base(client, navigation)
        {
        }

        public Task<LoginResponse> LoginAsync(string accessCode, CancellationToken cancellationToken = default)
        {
            LoginRequest request = new LoginRequest(accessCode);
            return base.Client.PostJsonAsync<LoginResponse>("auth/token", request, cancellationToken);
        }

        public Task<LoginResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshRequest request = new RefreshRequest(refreshToken);
            return base.Client.PostJsonAsync<LoginResponse>("auth/token/refresh", request, cancellationToken);
        }

        public Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
        {
            RefreshRequest request = new RefreshRequest(refreshToken);
            return base.Client.DeleteJsonAsync("auth/token", request, cancellationToken);
        }
    }
}
