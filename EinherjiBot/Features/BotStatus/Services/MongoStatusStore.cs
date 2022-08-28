using MongoDB.Driver;
using TehGM.EinherjiBot.Database;

namespace TehGM.EinherjiBot.BotStatus.Services
{
    public class MongoStatusStore : IStatusStore
    {
        private readonly IMongoCollection<BotStatus> _collection;
        private readonly ILogger _log;

        public MongoStatusStore(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, ILogger<MongoStatusStore> log)
        {
            this._collection = databaseConnection.GetCollection<BotStatus>(databaseOptions.CurrentValue.RandomStatusCollectionName);
            this._log = log;
        }

        public async Task<IEnumerable<BotStatus>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Retrieving all statuses from database");
            return await this._collection.Find(Builders<BotStatus>.Filter.Empty, null).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task<BotStatus> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Retrieving status {StatusID} from database", id);
            return this._collection.Find(db => db.ID == id, null).FirstOrDefaultAsync(cancellationToken);
        }

        public Task UpsertAsync(BotStatus status, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Saving status {StatusID} in database", status.ID);
            ReplaceOptions options = new ReplaceOptions() { IsUpsert = true };
            return this._collection.ReplaceOneAsync(db => db.ID == status.ID, status, options, cancellationToken);
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Deleting status {StatusID} from database", id);
            return this._collection.DeleteOneAsync(db => db.ID == id, cancellationToken);
        }
    }
}
