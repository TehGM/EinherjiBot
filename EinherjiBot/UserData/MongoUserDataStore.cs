using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Database;
using TehGM.EinherjiBot.Database.Services;

namespace TehGM.EinherjiBot.Services
{
    public class MongoUserDataStore : MongoBatchingRepositoryBase<ulong, UserData>, IBatchingRepository, IUserDataStore
    {
        private readonly IEntityCache<ulong, UserData> _cache;
        private readonly ILogger _log;
        private readonly ReplaceOptions _replaceOptions = new ReplaceOptions() { IsUpsert = true };
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        protected override TimeSpan BatchDelay => TimeSpan.FromMinutes(5);
        protected override IMongoCollection<UserData> Collection => base.MongoConnection
                .GetCollection<UserData>(base.DatabaseOptions.CurrentValue.UsersDataCollectionName);

        public MongoUserDataStore(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, IHostApplicationLifetime hostLifetime, ILogger<MongoUserDataStore> log, IEntityCache<ulong, UserData> cache)
            : base(databaseConnection, databaseOptions, hostLifetime, log)
        {
            this._cache = cache;
            this._log = log;
        }

        public async Task<UserData> GetAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                UserData result = this._cache.Get(userID);
                if (result != null)
                {
                    this._log.LogTrace("User data for user {UserID} found in cache", userID);
                    return result;
                }

                // get from DB
                this._log.LogTrace("Retrieving user data for user {UserID} from database", userID);
                result = await this.Collection.Find(dbData => dbData.ID == userID).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                // if not found, return default data
                if (result == null)
                {
                    this._log.LogTrace("User data for user {UserID} not found, creating new with defaults", userID);
                    result = new UserData(userID);
                }

                this._cache.AddOrReplace(result);
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
                this._cache.AddOrReplace(data);
                await base.BatchInserter.BatchAsync(data.ID, new MongoDelayedInsert<UserData>(dbData => dbData.ID == data.ID, data, this._replaceOptions), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                this._lock.Release();
            }
        }
    }
}
