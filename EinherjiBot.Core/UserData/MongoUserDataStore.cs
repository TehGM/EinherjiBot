using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Database;
using TehGM.EinherjiBot.Database.Services;

namespace TehGM.EinherjiBot.Services
{
    public class MongoUserDataStore : MongoBatchingRepositoryBase<ulong, UserData>, IBatchingRepository, IUserDataStore, IDisposable
    {
        public const string CacheOptionName = "UserData";
        private readonly IEntityCache<ulong, UserData> _userDataCache;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<CachingOptions> _cachingOptions;
        private IMongoCollection<UserData> _collection;
        private readonly ReplaceOptions _replaceOptions = new ReplaceOptions() { IsUpsert = true };
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public MongoUserDataStore(IMongoConnection databaseConnection, IOptionsMonitor<DatabaseOptions> databaseOptions, IHostApplicationLifetime hostLifetime, ILogger<MongoUserDataStore> log, IEntityCache<ulong, UserData> userDataCache, IOptionsMonitor<CachingOptions> cachingOptions)
            : base(databaseConnection, databaseOptions, hostLifetime, log)
        {
            this._userDataCache = userDataCache;
            this._log = log;
            this._cachingOptions = cachingOptions;

            base.MongoConnection.ClientChanged += OnClientChanged;
            OnClientChanged(base.MongoConnection.Client);
        }

        private void OnClientChanged(MongoClient client)
        {
            DatabaseOptions options = base.DatabaseOptions.CurrentValue;
            this._collection = base.MongoConnection.Client
                .GetDatabase(options.DatabaseName)
                .GetCollection<UserData>(options.UsersDataCollectionName);
            base.RecreateBatchInserter(_cachingOptions.Get(CacheOptionName).Lifetime, this._collection);
        }

        public async Task<UserData> GetAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                UserData result;
                CachingOptions cachingOptions = _cachingOptions.Get(CacheOptionName);
                if (cachingOptions.Enabled && (result = _userDataCache.Get(userID)) != null)
                {
                    _log.LogTrace("User data for user {UserID} found in cache", userID);
                    return result;
                }

                // get from DB
                _log.LogTrace("Retrieving user data for user {UserID} from database", userID);
                result = await _collection.Find(dbData => dbData.ID == userID).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                // if not found, return default data
                if (result == null)
                {
                    _log.LogTrace("User data for user {UserID} not found, creating new with defaults", userID);
                    result = new UserData(userID);
                }

                _userDataCache.AddOrReplace(result.ID, result, cachingOptions.Lifetime);
                return result;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SetAsync(UserData data, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _log.LogTrace("Inserting user data for user {UserID} into next DB batch", data.ID);
                _userDataCache.AddOrReplace(data.ID, data, _cachingOptions.Get(CacheOptionName).Lifetime);
                await base.BatchInserter.BatchAsync(data.ID, new MongoDelayedInsert<UserData>(dbData => dbData.ID == data.ID, data, _replaceOptions), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            try { base.MongoConnection.ClientChanged -= OnClientChanged; } catch { }
        }
    }
}
