using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.UI.API.Services
{
    public class WebUserInfoService : IUserInfoService
    {
        private readonly IApiClient _client;
        private UserInfoResponse _cachedBotInfo;

        public WebUserInfoService(IApiClient client)
        {
            this._client = client;
        }

        public async ValueTask<UserInfoResponse> GetBotInfoAsync(CancellationToken cancellationToken = default)
        {
            if (this._cachedBotInfo == null)
                this._cachedBotInfo = await this._client.GetJsonAsync<UserInfoResponse>("user/info/bot", cancellationToken).ConfigureAwait(false);
            return this._cachedBotInfo;
        }

        public Task<UserInfoResponse> GetUserInfoAsync(ulong userID, CancellationToken cancellationToken = default)
            => this._client.GetJsonAsync<UserInfoResponse>($"user/info/{userID}", cancellationToken);
    }
}