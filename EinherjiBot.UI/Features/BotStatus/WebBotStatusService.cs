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
            => this._client.PostJsonAsync<BotStatusResponse>("bot/status", request, cancellationToken);

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => this._client.DeleteAsync($"bot/status/{id}", null, cancellationToken);

        public Task<IEnumerable<BotStatusResponse>> GetAllAsync(CancellationToken cancellationToken = default)
            => this._client.GetJsonAsync<IEnumerable<BotStatusResponse>>("bot/status", cancellationToken);

        public Task<BotStatusResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
            => this._client.GetJsonAsync<BotStatusResponse>($"bot/status/{id}", cancellationToken);

        public Task<BotStatusResponse> UpdateAsync(Guid id, BotStatusRequest request, CancellationToken cancellationToken = default)
            => this._client.PutJsonAsync<BotStatusResponse>($"bot/status/{id}", request, cancellationToken);

        public Task SetCurrentAsync(BotStatusRequest request, CancellationToken cancellationToken = default)
            => this._client.PostAsync("bot/status/current", request, cancellationToken);
    }
}
