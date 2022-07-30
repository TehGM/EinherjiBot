using MongoDB.Driver;
using TehGM.EinherjiBot.Database;

namespace TehGM.EinherjiBot.MessageTriggers.Services
{
    public class MongoMessageTriggersStore : IMessageTriggersStore
    {
        private readonly IMongoCollection<MessageTrigger> _collection;
        private readonly ILogger _log;

        public MongoMessageTriggersStore(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, ILogger<MongoMessageTriggersStore> log)
        {
            this._collection = databaseConnection.GetCollection<MessageTrigger>(databaseOptions.CurrentValue.MessageTriggersCollectionName);
            this._log = log;
        }

        public async Task<IEnumerable<MessageTrigger>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Getting all message triggers from DB");
            return await this._collection.Find(Builders<MessageTrigger>.Filter.Empty).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task<MessageTrigger> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Getting message trigger {ID} from DB", id);
            return this._collection.Find(db => db.ID == id).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<MessageTrigger>> GetForGuildAsync(ulong guildID, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Getting message triggers for guild {ID} from DB", guildID);
            return await this._collection.Find(db => db.GuildID == guildID).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<IEnumerable<MessageTrigger>> GetGlobalsAsync(CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Getting global message triggers from DB");
            return await this._collection.Find(db => db.GuildID == MessageTrigger.GlobalGuildID).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task UpdateAsync(MessageTrigger trigger, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Upserting message trigger {ID} to DB", trigger.ID);
            ReplaceOptions options = new ReplaceOptions() { IsUpsert = true };
            return this._collection.ReplaceOneAsync(db => db.ID == trigger.ID, trigger, options, cancellationToken);
        }
    }
}
