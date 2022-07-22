using TehGM.EinherjiBot.Caching;

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
                return result.CanAccess(this._auth) ? result : null;
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
                await this.PopulateCacheAsync(cancellationToken).ConfigureAwait(false);

                IEnumerable<GameServer> results = this._cache.Find(s => s.CanAccess(this._auth));
                if (results.Any())
                    this._log.LogTrace("{Count} game servers found in cache", results.Count());
                return results;
            }
            finally
            {
                this._lock.Release();
            }
        }

        private async Task PopulateCacheAsync(CancellationToken cancellationToken)
        {
            this._cache.ClearExpired();
            if (this._cache.CachedCount != 0)
                return;

            IEnumerable<GameServer> results = await this._store.FindAsync(null, null, null, cancellationToken).ConfigureAwait(false);
            foreach (GameServer result in results)
                this._cache.AddOrReplace(result);
        }

        public async Task UpdateAsync(GameServer server, CancellationToken cancellationToken = default)
        {
            GameServer existing = await this.GetAsync(server.ID, cancellationToken).ConfigureAwait(false);
            if (existing != null && !existing.CanEdit(this._auth))
                throw new InvalidOperationException($"No permissions to edit game server {server.ID}");
            await this._store.UpdateAsync(server, cancellationToken).ConfigureAwait(false);
            this._cache.AddOrReplace(server);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            GameServer existing = await this.GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (existing != null && !existing.CanEdit(this._auth))
                throw new InvalidOperationException($"No permissions to delete game server {id}");
            await this._store.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            this._cache.Remove(id);
        }

        public void Dispose()
        {
            try { this._lock?.Dispose(); } catch { }
        }
    }
}
