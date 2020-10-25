using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Database;

namespace TehGM.EinherjiBot.Netflix.Services
{
    public class NetflixAccountStore : INetflixAccountStore, IDisposable
    {
        // caching
        public const string CacheOptionName = "NetflixAccount";
        private CachedEntity<string, NetflixAccount> _cachedAccount;
        private readonly IOptionsMonitor<CachingOptions> _cachingOptions;
        // db
        private readonly IMongoConnection _databaseConnection;
        private IMongoCollection<NetflixAccount> _netflixAccountsCollection;
        private readonly IOptionsMonitor<DatabaseOptions> _databaseOptions;
        private readonly ReplaceOptions _replaceOptions = new ReplaceOptions() { IsUpsert = true };
        // misc
        private readonly IOptionsMonitor<NetflixAccountOptions> _netflixAccountOptions;
        private readonly ILogger _log;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);


        public NetflixAccountStore(IMongoConnection databaseConnection, ILogger<NetflixAccountStore> log,
            IOptionsMonitor<DatabaseOptions> databaseOptions, IOptionsMonitor<CachingOptions> cachingOptions, IOptionsMonitor<NetflixAccountOptions> netflixAccountOptions)
        {
            this._databaseConnection = databaseConnection;
            this._databaseOptions = databaseOptions;
            this._cachingOptions = cachingOptions;
            this._netflixAccountOptions = netflixAccountOptions;
            this._log = log;

            this._databaseConnection.ClientChanged += OnDatabaseChanged;
        }

        private void OnDatabaseChanged(MongoClient client)
        {
            this._netflixAccountsCollection = client
                .GetDatabase(this._databaseOptions.CurrentValue.DatabaseName)
                .GetCollection<NetflixAccount>(this._netflixAccountOptions.CurrentValue.DatabaseCollectionName);
        }

        public async Task<NetflixAccount> GetAsync(CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                CachingOptions cachingOptions = _cachingOptions.Get(CacheOptionName);
                if (cachingOptions.Enabled && _cachedAccount != null && !_cachedAccount.IsExpired)
                {
                    _log.LogTrace("Netflix account found in cache");
                    return _cachedAccount;
                }

                _log.LogDebug("Retrieving Netflix account from database");
                NetflixAccount result = await _netflixAccountsCollection.Find(_ => true).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
                if (result == null)
                {
                    _log.LogTrace("Netflix account not found, creating default");
                    result = new NetflixAccount();
                }

                if (cachingOptions.Enabled)
                    _cachedAccount = new CachedEntity<string, NetflixAccount>(result.Login, result, cachingOptions.Lifetime);
                return result;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SetAsync(NetflixAccount account, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                CachingOptions cachingOptions = _cachingOptions.Get(CacheOptionName);
                if (cachingOptions.Enabled)
                {
                    _log.LogTrace("Updating cached Netflix account");
                    _cachedAccount = new CachedEntity<string, NetflixAccount>(account.Login, account, cachingOptions.Lifetime);
                }

                _log.LogTrace("Saving Netflix account in the database");
                await _netflixAccountsCollection.ReplaceOneAsync(_ => true, account, _replaceOptions, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
        }

        public void Dispose()
        {
            this._databaseConnection.ClientChanged -= OnDatabaseChanged;
        }
    }
}
