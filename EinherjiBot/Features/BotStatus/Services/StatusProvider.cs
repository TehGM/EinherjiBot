using TehGM.EinherjiBot.Caching;

namespace TehGM.EinherjiBot.BotStatus.Services
{
    public class StatusProvider : IStatusProvider, IDisposable
    {
        private readonly IStatusStore _store;
        private readonly IEntityCache<Guid, Status> _cache;
        private readonly ILogger _log;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public StatusProvider(IStatusStore store, IEntityCache<Guid, Status> cache, ILogger<StatusProvider> log)
        {
            this._store = store;
            this._cache = cache;
            this._log = log;

            this._cache.DefaultExpiration = new TimeSpanEntityExpiration(TimeSpan.FromHours(1));
        }

        public async Task<IEnumerable<Status>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                IEnumerable<Status> results = this._cache.Find(_ => true);
                if (results.Any())
                {
                    this._log.LogTrace("{Count} random statuses found in cache", results.Count());
                    return results;
                }

                results = await this._store.GetAllAsync(cancellationToken).ConfigureAwait(false);

                if (results.Any())
                {
                    foreach (Status result in results)
                        this._cache.AddOrReplace(result);
                }
                return results;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task<Status> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                Status result = this._cache.Get(id);
                if (result != null)
                {
                    this._log.LogTrace("Random status {StatusID} found in cache", id);
                    return result;
                }

                result = await this._store.GetAsync(id, cancellationToken).ConfigureAwait(false);

                if (result != null)
                    this._cache.AddOrReplace(result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task UpdateAsync(Status status, CancellationToken cancellationToken = default)
        {
            await this._store.UpdateAsync(status, cancellationToken).ConfigureAwait(false);
            this._cache.AddOrReplace(status);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await this._store.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            this._cache.Remove(id);
        }

        public void Dispose()
        {
            try { this._lock?.Dispose(); } catch { }
        }
    }
}
