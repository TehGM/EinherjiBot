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
    public class MongoNetflixAccountStore : INetflixAccountStore
    {
        // caching
        public const string CacheOptionName = "NetflixAccount";
        private CachedEntity<string, NetflixAccount> _cachedAccount;
        private readonly IOptionsMonitor<CachingOptions> _cachingOptions;
        // db
        private readonly IMongoConnection _databaseConnection;
        private IMongoCollection<NetflixAccount> _netflixAccountsCollection;
        private readonly IOptionsMonitor<MongoOptions> _databaseOptions;
        private readonly ReplaceOptions _replaceOptions = new ReplaceOptions() { IsUpsert = true };
        // misc
        private readonly IOptionsMonitor<NetflixAccountOptions> _netflixAccountOptions;
        private readonly ILogger _log;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);


        public MongoNetflixAccountStore(IMongoConnection databaseConnection, ILogger<MongoNetflixAccountStore> log,
            IOptionsMonitor<MongoOptions> databaseOptions, IOptionsMonitor<CachingOptions> cachingOptions, IOptionsMonitor<NetflixAccountOptions> netflixAccountOptions)
        {
            this._databaseConnection = databaseConnection;
            this._databaseOptions = databaseOptions;
            this._cachingOptions = cachingOptions;
            this._netflixAccountOptions = netflixAccountOptions;
            this._log = log;

            this._netflixAccountsCollection = databaseConnection
                .GetCollection<NetflixAccount>(this._netflixAccountOptions.CurrentValue.DatabaseCollectionName);
        }

        public async Task<NetflixAccount> GetAsync(CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                CachingOptions cachingOptions = _cachingOptions.Get(CacheOptionName);
                if (cachingOptions.Enabled && this._cachedAccount != null && !this._cachedAccount.IsExpired)
                {
                    this._log.LogTrace("Netflix account found in cache");
                    return this._cachedAccount;
                }

                this._log.LogDebug("Retrieving Netflix account from database");
                NetflixAccount result = await this._netflixAccountsCollection.Find(_ => true).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
                if (result == null)
                {
                    this._log.LogTrace("Netflix account not found, creating default");
                    result = new NetflixAccount();
                }

                if (cachingOptions.Enabled)
                    this._cachedAccount = new CachedEntity<string, NetflixAccount>(result.Login, result, cachingOptions.Lifetime);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task UpdateAsync(NetflixAccount account, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                CachingOptions cachingOptions = this._cachingOptions.Get(CacheOptionName);
                if (cachingOptions.Enabled)
                {
                    this._log.LogTrace("Updating cached Netflix account");
                    this._cachedAccount = new CachedEntity<string, NetflixAccount>(account.Login, account, cachingOptions.Lifetime);
                }

                this._log.LogTrace("Saving Netflix account in the database");
                await this._netflixAccountsCollection.ReplaceOneAsync(_ => true, account, this._replaceOptions, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                this._lock.Release();
            }
        }
    }
}
