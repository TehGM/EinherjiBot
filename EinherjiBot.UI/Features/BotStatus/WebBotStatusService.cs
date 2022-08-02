using System.Net.Http;
using TehGM.EinherjiBot.BotStatus.API;
using TehGM.EinherjiBot.UI.API;

namespace TehGM.EinherjiBot.UI.BotStatus.API
{
    public class WebBotStatusService : IBotStatusService
    {
        private IApiClient _client;

        public WebBotStatusService(IApiClient client)
        {
            this._client = client;
        }

        public Task<BotStatusResponse> CreateAsync(BotStatusRequest request, CancellationToken cancellationToken = default)
            => this._client.Client.PostJsonAsync<BotStatusResponse>("bot/status", request, cancellationToken);

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => this._client.Client.DeleteAsync($"bot/status/{id}", cancellationToken);

        public Task<IEnumerable<BotStatusResponse>> GetAllAsync(CancellationToken cancellationToken = default)
            => this._client.Client.GetJsonAsync<IEnumerable<BotStatusResponse>>("bot/status", cancellationToken);

        public Task<BotStatusResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
            => this._client.Client.GetJsonAsync<BotStatusResponse>($"bot/status/{id}", cancellationToken);

        public Task<BotStatusResponse> UpdateAsync(Guid id, BotStatusRequest request, CancellationToken cancellationToken = default)
            => this._client.Client.PutJsonAsync<BotStatusResponse>($"bot/status/{id}", request, cancellationToken);

        public Task SetCurrentAsync(BotStatusRequest request, CancellationToken cancellationToken = default)
            => this._client.Client.PostJsonAsync("bot/status/current", request, cancellationToken);
    }
}
