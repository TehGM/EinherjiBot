using Discord;
using TehGM.EinherjiBot.Caching;

namespace TehGM.EinherjiBot.Security.Services
{
    public class DiscordSocketUserContextProvider : IUserContextProvider, IDisposable
    {
        private readonly IDiscordClient _client;
        private readonly IUserSecurityDataStore _store;
        private readonly IEntityCache<ulong, UserSecurityData> _userDataCache;
        private readonly ILogger _log;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public DiscordSocketUserContextProvider(IDiscordClient client, IUserSecurityDataStore store, 
            IEntityCache<ulong, UserSecurityData> userDataCache, ILogger<DiscordSocketUserContextProvider> log)
        {
            this._client = client;
            this._store = store;
            this._userDataCache = userDataCache;
            this._log = log;

            this._userDataCache.DefaultExpiration = new TimeSpanEntityExpiration(TimeSpan.FromMinutes(5));
        }

        public async Task<IUserContext> GetUserContextAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            Task<UserSecurityData> dataTask = this.GetUserSecurityDataAsync(userID, cancellationToken);
            Task<IUser> userTask = this._client.GetUserAsync(userID, cancellationToken);
            await Task.WhenAll(dataTask, userTask).ConfigureAwait(false);

            return new DiscordSocketUserContext(userTask.Result, dataTask.Result);
        }

        private async Task<UserSecurityData> GetUserSecurityDataAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                UserSecurityData result = this._userDataCache.Get(userID);
                if (result != null)
                {
                    this._log.LogTrace("User security data for user {UserID} found in cache", userID);
                    return result;
                }

                result = await this._store.GetAsync(userID, cancellationToken).ConfigureAwait(false);

                if (result == null)
                {
                    this._log.LogTrace("User intel for user {UserID} not found, creating new with defaults", userID);
                    result = new UserSecurityData(userID);
                }

                this._userDataCache.AddOrReplace(result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public void Dispose()
        {
            try { this._lock?.Dispose(); } catch { }
        }
    }
}
