using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Caching;

namespace TehGM.EinherjiBot.SharedAccounts.Services
{
    public class SharedAccountProvider : ISharedAccountProvider
    {
        private readonly IBotAuthorizationService _authService;
        private readonly ISharedAccountStore _store;
        private readonly IEntityCache<Guid, SharedAccount> _cache;
        private readonly ILogger _log;
        private readonly ILockProvider _lock;

        public SharedAccountProvider(ISharedAccountStore store, IEntityCache<Guid, SharedAccount> cache, 
            IBotAuthorizationService authService, ILogger<SharedAccountProvider> log, ILockProvider<SharedAccountProvider> lockProvider)
        {
            this._store = store;
            this._cache = cache;
            this._log = log;
            this._authService = authService;
            this._lock = lockProvider;

            this._cache.DefaultExpiration = new TimeSpanEntityExpiration(TimeSpan.FromHours(1));
        }

        public async Task<IEnumerable<SharedAccount>> FindAsync(SharedAccountFilter filter, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                using IDisposable logScope = this._log.BeginScope(new Dictionary<string, object>()
                {
                    { nameof(filter.AccountType), filter.AccountType },
                    { nameof(filter.LoginStartsWith), filter.LoginStartsWith },
                    { nameof(filter.LoginContains), filter.LoginContains },
                    { nameof(filter.AuthorizeUserID), filter.AuthorizeUserID },
                    { nameof(filter.AuthorizeRoleIDs), string.Join(',', filter.AuthorizeRoleIDs ?? Enumerable.Empty<ulong>()) },
                    { nameof(filter.ModUserID), filter.ModUserID },
                });

                await this.PopulateCacheAsync(cancellationToken).ConfigureAwait(false);

                IEnumerable<SharedAccount> results = filter.Filter(this._cache.Find(_ => true)).Cast<SharedAccount>();
                if (results.Any())
                    this._log.LogTrace("{Count} shared accounts found in cache", results.Count());
                return results;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task<SharedAccount> GetAsync(Guid id, CancellationToken cancellationToken)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                return await this.GetInternalAsync(id, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                this._lock.Release();
            }
        }

        private async Task<SharedAccount> GetInternalAsync(Guid id, CancellationToken cancellationToken)
        {
            await this.PopulateCacheAsync(cancellationToken).ConfigureAwait(false);
            return this._cache.Get(id);
        }

        public async Task AddOrUpdateAsync(SharedAccount account, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await this._store.UpdateAsync(account, cancellationToken).ConfigureAwait(false);
                this._cache.AddOrReplace(account);
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
                SharedAccount account = await this.GetInternalAsync(id, cancellationToken).ConfigureAwait(false);
                if (account == null)
                    return;

                await this._store.DeleteAsync(id, cancellationToken).ConfigureAwait(false);
                this._cache.Remove(id);
            }
            finally
            {
                this._lock.Release();
            }
        }

        // to allow GetAll to work with cache, we need to always have cache populated with all entites
        // this is not ideal, but in case of shared accounts, it's acceptable
        // maybe later can make compound cache using the filter object?
        private async Task PopulateCacheAsync(CancellationToken cancellationToken)
        {
            this._cache.ClearExpired();
            if (this._cache.CachedCount != 0)
                return;

            IEnumerable<SharedAccount> results = await this._store.FindAsync(SharedAccountFilter.Empty, cancellationToken).ConfigureAwait(false);
            foreach (SharedAccount result in results)
                this._cache.AddOrReplace(result);
        }
    }
}
