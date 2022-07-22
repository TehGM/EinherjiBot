using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.GameServers.Services
{
    public class GameServerProvider : IGameServerProvider
    {
        private readonly IGameServerStore _store;
        private readonly IEntityCache<Guid, GameServer> _cache;
        private readonly IAuthContext _auth;
        private readonly ILogger _log;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public GameServerProvider(IGameServerStore store, IEntityCache<Guid, GameServer> cache, IAuthContext auth, ILogger<GameServerProvider> log)
        {
            this._store = store;
            this._cache = cache;
            this._log = log;
            this._auth = auth;

            this._cache.DefaultExpiration = new TimeSpanEntityExpiration(TimeSpan.FromHours(1));
        }

        public async Task<GameServer> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                GameServer result = this._cache.Get(id);
                if (result != null)
                {
                    this._log.LogTrace("Game server {ServerID} found in cache", id);
                    return result;
                }

                result = await this._store.GetAsync(id, cancellationToken).ConfigureAwait(false);

                if (result != null)
                    this._cache.AddOrReplace(result);
                return result.IsAuthorized(this._auth) ? result : null;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task<IEnumerable<GameServer>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                IEnumerable<GameServer> results = this._cache.Find(_ => true);
                if (results.Any())
                {
                    this._log.LogTrace("{Count} game servers found in cache", results.Count());
                    return results;
                }

                results = await this._store.FindAsync(null, null, null, cancellationToken).ConfigureAwait(false);

                if (results.Any())
                {
                    foreach (GameServer result in results)
                        this._cache.AddOrReplace(result);
                }
                return results.Where(s => s.IsAuthorized(this._auth));
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task UpdateAsync(GameServer server, CancellationToken cancellationToken = default)
        {
            await this._store.UpdateAsync(server, cancellationToken).ConfigureAwait(false);
            this._cache.AddOrReplace(server);
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
