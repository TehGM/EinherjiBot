using TehGM.EinherjiBot.Settings;
using TehGM.EinherjiBot.UI.API;

namespace TehGM.EinherjiBot.UI.Settings
{
    public class WebGuildSettingsHandler : IGuildSettingsHandler
    {
        private readonly IApiClient _client;

        public WebGuildSettingsHandler(IApiClient client)
        {
            this._client = client;
        }

        public Task<GuildSettingsResponse> GetAsync(ulong guildID, CancellationToken cancellationToken = default)
            => this._client.GetJsonAsync<GuildSettingsResponse>($"guild/{guildID}/settings", cancellationToken);

        public async Task<EntityUpdateResult<GuildSettingsResponse>> UpdateAsync(ulong guildID, GuildSettingsRequest request, CancellationToken cancellationToken = default)
        {
            GuildSettingsResponse response = await this._client.PutJsonAsync<GuildSettingsResponse>($"guild/{guildID}/settings", request, cancellationToken);
            return IEntityUpdateResult.Saved(response);
        }
    }
}
