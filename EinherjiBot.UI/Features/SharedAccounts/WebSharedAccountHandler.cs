using TehGM.EinherjiBot.SharedAccounts;
using TehGM.EinherjiBot.UI.API;

namespace TehGM.EinherjiBot.UI.SharedAccounts
{
    public class WebSharedAccountHandler : ISharedAccountHandler
    {
        private readonly IApiClient _client;

        public WebSharedAccountHandler(IApiClient client)
        {
            this._client = client;
        }

        public Task<IEnumerable<SharedAccountResponse>> GetAllAsync(SharedAccountFilter filter, bool skipAudit, CancellationToken cancellationToken = default)
            => this._client.GetJsonAsync<IEnumerable<SharedAccountResponse>>("shared-accounts", cancellationToken);

        public Task<SharedAccountResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
            => this._client.GetJsonAsync<SharedAccountResponse>($"shared-accounts/{id}", cancellationToken);

        public Task<SharedAccountResponse> CreateAsync(SharedAccountRequest request, CancellationToken cancellationToken = default)
            => this._client.PostJsonAsync<SharedAccountResponse>("shared-accounts", request, cancellationToken);

        public async Task<EntityUpdateResult<SharedAccountResponse>> UpdateAsync(Guid id, SharedAccountRequest request, CancellationToken cancellationToken = default)
        {
            SharedAccountResponse response = await this._client.PutJsonAsync<SharedAccountResponse>($"shared-accounts/{id}", request, cancellationToken);
            return IEntityUpdateResult.Saved(response);
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => this._client.DeleteAsync($"shared-accounts/{id}", null, cancellationToken);

        public Task<IDictionary<SharedAccountType, string>> GetImagesAsync(CancellationToken cancellationToken = default)
            => this._client.GetJsonAsync<IDictionary<SharedAccountType, string>>("shared-accounts/images", cancellationToken);
    }
}
