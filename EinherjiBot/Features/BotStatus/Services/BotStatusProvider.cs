using TehGM.EinherjiBot.Caching;

namespace TehGM.EinherjiBot.BotStatus.Services
{
    public class BotStatusProvider : IBotStatusProvider
    {
        private readonly IStatusStore _store;
        private readonly IEntityCache<Guid, BotStatus> _cache;
        private readonly ILockProvider _lock;
        private readonly ILogger _log;

        public BotStatusProvider(IStatusStore store, IEntityCache<Guid, BotStatus> cache, 
            ILockProvider<BotStatusProvider> lockProvider, ILogger<BotStatusProvider> log)
        {
            this._store = store;
            this._cache = cache;
            this._lock = lockProvider;
            this._log = log;

            this._cache.DefaultExpiration = new TimeSpanEntityExpiration(TimeSpan.FromHours(1));
        }

        public async Task<IEnumerable<BotStatus>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                IEnumerable<BotStatus> results = this._cache.Find(_ => true);
                if (results.Any())
                {
                    this._log.LogTrace("{Count} random statuses found in cache", results.Count());
                    return results;
                }

                results = await this._store.GetAllAsync(cancellationToken).ConfigureAwait(false);

                if (results.Any())
                {
                    foreach (BotStatus result in results)
                        this._cache.AddOrReplace(result);
                }
                return results;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task<BotStatus> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                BotStatus result = this._cache.Get(id);
                if (result != null)
                {
                    this._log.LogTrace("Random status {StatusID} found in cache", id);
                    return result;
                }

                result = await this._store.GetAsync(id, cancellationToken).ConfigureAwait(false);
                this._cache.AddOrReplace(id, result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task AddOrUpdateAsync(BotStatus status, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await this._store.UpsertAsync(status, cancellationToken).ConfigureAwait(false);
                this._cache.AddOrReplace(status);
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await this._store.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
                this._cache.Remove(id);
            }
            finally
            {
                this._lock.Release();
            }
        }
    }
}
