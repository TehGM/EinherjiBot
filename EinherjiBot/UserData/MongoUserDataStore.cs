using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Database;
using TehGM.EinherjiBot.Database.Services;

namespace TehGM.EinherjiBot.Services
{
    public class MongoUserDataStore : MongoBatchingRepositoryBase<ulong, UserData>, IBatchingRepository, IUserDataStore
    {
        public const string CacheOptionName = "UserData";
        private readonly IEntityCache<ulong, UserData> _userDataCache;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<CachingOptions> _cachingOptions;
        private IMongoCollection<UserData> _collection;
        private readonly ReplaceOptions _replaceOptions = new ReplaceOptions() { IsUpsert = true };
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        protected override TimeSpan BatchDelay => TimeSpan.FromMinutes(5);
        protected override IMongoCollection<UserData> Collection => base.MongoConnection
                .GetCollection<UserData>(base.DatabaseOptions.CurrentValue.UsersDataCollectionName);

        public MongoUserDataStore(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, IHostApplicationLifetime hostLifetime, ILogger<MongoUserDataStore> log, IEntityCache<ulong, UserData> userDataCache, IOptionsMonitor<CachingOptions> cachingOptions)
            : base(databaseConnection, databaseOptions, hostLifetime, log)
        {
            this._userDataCache = userDataCache;
            this._log = log;
            this._cachingOptions = cachingOptions;
        }

        public async Task<UserData> GetAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                UserData result;
                CachingOptions cachingOptions = this._cachingOptions.Get(CacheOptionName);
                if (cachingOptions.Enabled && (result = this._userDataCache.Get(userID)) != null)
                {
                    this._log.LogTrace("User data for user {UserID} found in cache", userID);
                    return result;
                }

                // get from DB
                this._log.LogTrace("Retrieving user data for user {UserID} from database", userID);
                result = await this._collection.Find(dbData => dbData.ID == userID).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                // if not found, return default data
                if (result == null)
                {
                    this._log.LogTrace("User data for user {UserID} not found, creating new with defaults", userID);
                    result = new UserData(userID);
                }

                this._userDataCache.AddOrReplace(result.ID, result, cachingOptions.Lifetime);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task UpdateAsync(UserData data, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this._log.LogTrace("Inserting user data for user {UserID} into next DB batch", data.ID);
                this._userDataCache.AddOrReplace(data.ID, data, this._cachingOptions.Get(CacheOptionName).Lifetime);
                await base.BatchInserter.BatchAsync(data.ID, new MongoDelayedInsert<UserData>(dbData => dbData.ID == data.ID, data, this._replaceOptions), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                this._lock.Release();
            }
        }
    }
}
