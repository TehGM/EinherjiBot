using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Caching;

namespace TehGM.EinherjiBot.UI.API.Services
{
    public class WebUserInfoService : IUserInfoService
    {
        private readonly IApiClient _client;
        private readonly IEntityCache<UserInfoResponse> _cache;
        private readonly ILockProvider _lock;

        private UserInfoResponse _cachedBotInfo;

        public WebUserInfoService(IApiClient client, IEntityCache<UserInfoResponse> cache, ILockProvider<WebUserInfoService> lockProvider)
        {
            this._client = client;
            this._cache = cache;
            this._lock = lockProvider;
        }

        public async ValueTask<UserInfoResponse> GetBotInfoAsync(CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (this._cachedBotInfo == null)
                {
                    this._cachedBotInfo = await this._client.GetJsonAsync<UserInfoResponse>("user/info/bot", cancellationToken).ConfigureAwait(false);
                    this._cache.AddOrReplace(this._cachedBotInfo.ID, this._cachedBotInfo, new TimeSpanEntityExpiration(TimeSpan.FromMilliseconds(30)));
                }
                return this._cachedBotInfo;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task<UserInfoResponse> GetUserInfoAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (this._cache.TryGet(userID, out UserInfoResponse result))
                    return result;

                result = await this._client.GetJsonAsync<UserInfoResponse>($"user/info/{userID}", cancellationToken).ConfigureAwait(false);
                this._cache.AddOrReplace(userID, result, new SlidingEntityExpiration(TimeSpan.FromMinutes(5)));
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }
    }
}