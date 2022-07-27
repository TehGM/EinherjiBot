using Blazored.LocalStorage;
using System.Net.Http;
using TehGM.EinherjiBot.Security.API;
using TehGM.EinherjiBot.UI.API;

namespace TehGM.EinherjiBot.UI.Security.Services
{
    public class WebAuthService : IAuthService
    {
        private readonly IApiClient _client;
        private readonly IWebAuthProvider _provider;
        private readonly ILocalStorageService _storage;

        public WebAuthService(IApiClient client, IWebAuthProvider provider, ILocalStorageService localStorage)
        {
            this._client = client;
            this._provider = provider;
            this._storage = localStorage;
        }

        public async Task<LoginResponse> LoginAsync(string accessCode, CancellationToken cancellationToken = default)
        {
            LoginRequest request = new LoginRequest(accessCode);
            LoginResponse response = await this._client.Client.PostJsonAsync<LoginResponse>("auth/token", request, cancellationToken).ConfigureAwait(false);
            this._provider.Login(response);
            return response;
        }
    }
}
