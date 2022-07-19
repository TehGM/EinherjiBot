using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Database;
using TehGM.EinherjiBot.Database.Services;

namespace TehGM.EinherjiBot.Intel.Services
{
    public class MongoUserOnlineHistoryStore : MongoBatchingRepositoryBase<ulong, UserOnlineHistory>, IBatchingRepository, IUserOnlineHistoryStore
    {
        private readonly ILogger _log;
        private readonly ReplaceOptions _replaceOptions = new ReplaceOptions() { IsUpsert = true };

        protected override TimeSpan BatchDelay => TimeSpan.FromMinutes(5);
        protected override IMongoCollection<UserOnlineHistory> Collection => base.MongoConnection
                .GetCollection<UserOnlineHistory>(base.DatabaseOptions.CurrentValue.UserIntelCollectionName);

        public MongoUserOnlineHistoryStore(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, IHostApplicationLifetime hostLifetime, ILogger<MongoUserOnlineHistoryStore> log)
            : base(databaseConnection, databaseOptions, hostLifetime, log)
        {
            this._log = log;
        }

        public Task<UserOnlineHistory> GetAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Retrieving user intel for user {UserID} from database", userID);
            return this.Collection.Find(dbData => dbData.ID == userID)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task UpdateAsync(UserOnlineHistory intel, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Inserting user intel for user {UserID} into next DB batch", intel.ID);
            return base.BatchInserter.BatchAsync(intel.ID, 
                new MongoDelayedInsert<UserOnlineHistory>(dbData => dbData.ID == intel.ID, intel, this._replaceOptions), 
                cancellationToken);
        }
    }
}
