using TehGM.EinherjiBot.Caching;

namespace TehGM.EinherjiBot.SharedAccounts.Services
{
    public class SharedAccountProvider : ISharedAccountProvider
    {
        private readonly ISharedAccountStore _store;
        private readonly IEntityCache<Guid, SharedAccount> _cache;
        private readonly IAuthContext _auth;
        private readonly ILogger _log;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public SharedAccountProvider(ISharedAccountStore store, IEntityCache<Guid, SharedAccount> cache, IAuthContext auth, ILogger<SharedAccountProvider> log)
        {
            this._store = store;
            this._cache = cache;
            this._log = log;
            this._auth = auth;

            this._cache.DefaultExpiration = new TimeSpanEntityExpiration(TimeSpan.FromHours(1));
        }

        public async Task<SharedAccount> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                SharedAccount result = this._cache.Get(id);
                if (result != null)
                {
                    this._log.LogTrace("Shared account {AccountID} found in cache", id);
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

        public async Task<IEnumerable<SharedAccount>> GetOfTypeAsync(SharedAccountType type, bool forModeration, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await this.PopulateCacheAsync(cancellationToken).ConfigureAwait(false);

                Func<SharedAccount, bool> filter = forModeration ? s => s.CanEdit(this._auth) : s => s.CanAccess(this._auth);
                IEnumerable<SharedAccount> results = this._cache.Find(s => filter(s));
                if (results.Any())
                    this._log.LogTrace("{Count} shared accounts found in cache", results.Count());
                return results;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task UpdateAsync(SharedAccount account, CancellationToken cancellationToken = default)
        {
            SharedAccount existing = await this.GetAsync(account.ID, cancellationToken).ConfigureAwait(false);
            if (existing != null && !existing.CanEdit(this._auth))
                throw new InvalidOperationException($"No permissions to edit shared account {account.ID}");
            await this._store.UpdateAsync(account, cancellationToken).ConfigureAwait(false);
            this._cache.AddOrReplace(account);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            SharedAccount existing = await this.GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (existing != null && !existing.CanEdit(this._auth))
                throw new InvalidOperationException($"No permissions to delete shared account {id}");
            await this._store.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
            this._cache.Remove(id);
        }

        private async Task PopulateCacheAsync(CancellationToken cancellationToken)
        {
            this._cache.ClearExpired();
            if (this._cache.CachedCount != 0)
                return;

            IEnumerable<SharedAccount> results = await this._store.FindAsync(null, null, null, false, cancellationToken).ConfigureAwait(false);
            foreach (SharedAccount result in results)
                this._cache.AddOrReplace(result);
        }

        public void Dispose()
        {
            try { this._lock?.Dispose(); } catch { }
        }
    }
}
