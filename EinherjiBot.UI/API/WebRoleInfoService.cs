using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Caching;

namespace TehGM.EinherjiBot.UI.API.Services
{
    public class WebRoleInfoService : IRoleInfoService
    {
        private readonly IApiClient _client;
        private readonly IEntityCache<RoleInfoResponse> _cache;
        private readonly ILockProvider _lock;

        public WebRoleInfoService(IApiClient client, IEntityCache<RoleInfoResponse> cache, ILockProvider<WebRoleInfoService> lockProvider)
        {
            this._client = client;
            this._cache = cache;
            this._lock = lockProvider;
        }

        public async Task<RoleInfoResponse> GetRoleInfoAsync(ulong id, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (this._cache.TryGet(id, out RoleInfoResponse result))
                    return result;

                result = await this._client.GetJsonAsync<RoleInfoResponse>($"role/info/{id}", cancellationToken).ConfigureAwait(false);
                this._cache.AddOrReplace(id, result, new SlidingEntityExpiration(TimeSpan.FromMinutes(5)));
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }
    }
}