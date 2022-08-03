using System.Net.Http;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.UI.API.Services
{
    public class WebUserInfoService : IUserInfoService
    {
        private readonly IApiClient _client;

        public WebUserInfoService(IApiClient client)
        {
            this._client = client;
        }

        public Task<UserInfoResponse> GetBotInfoAsync(CancellationToken cancellationToken = default)
            => this._client.GetJsonAsync<UserInfoResponse>("user/info/bot", cancellationToken);

        public Task<UserInfoResponse> GetUserInfoAsync(ulong userID, CancellationToken cancellationToken = default)
            => this._client.GetJsonAsync<UserInfoResponse>($"user/info/{userID}", cancellationToken);
    }
}