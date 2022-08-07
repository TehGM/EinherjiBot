using TehGM.EinherjiBot.SharedAccounts.API;
using TehGM.EinherjiBot.UI.API;

namespace TehGM.EinherjiBot.UI.SharedAccounts
{
    public class WebSharedAccountsService : ISharedAccountsService
    {
        private readonly IApiClient _client;

        public WebSharedAccountsService(IApiClient client)
        {
            this._client = client;
        }

        public Task<IEnumerable<SharedAccountResponse>> GetAllAsync(CancellationToken cancellationToken = default)
            => this._client.GetJsonAsync<IEnumerable<SharedAccountResponse>>("shared-accounts", cancellationToken);

        public Task<SharedAccountResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
            => this._client.GetJsonAsync<SharedAccountResponse>($"shared-accounts/{id}", cancellationToken);

        public Task<SharedAccountResponse> CreateAsync(SharedAccountRequest request, CancellationToken cancellationToken = default)
            => this._client.PostJsonAsync<SharedAccountResponse>("shared-accounts", request, cancellationToken);

        public Task<SharedAccountResponse> UpdateAsync(Guid id, SharedAccountRequest request, CancellationToken cancellationToken = default)
            => this._client.PutJsonAsync<SharedAccountResponse>($"shared-accounts/{id}", request, cancellationToken);

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => this._client.DeleteAsync($"shared-accounts/{id}", null, cancellationToken);
    }
}
