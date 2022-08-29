using MongoDB.Driver;
using TehGM.EinherjiBot.Database;

namespace TehGM.EinherjiBot.Settings.Services
{
    public class MongoGuildSettingsStore : IGuildSettingsStore
    {
        private readonly IMongoCollection<GuildSettings> _collection;
        private readonly ILogger _log;

        public MongoGuildSettingsStore(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, ILogger<MongoGuildSettingsStore> log)
        {
            this._log = log;
            this._collection = databaseConnection.GetCollection<GuildSettings>(databaseOptions.CurrentValue.GuildSettingsCollectionName);
        }

        public Task<GuildSettings> GetAsync(ulong guildID, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Retrieving settings for guild {GuildID} from database", guildID);
            return this._collection.Find(db => db.GuildID == guildID).FirstOrDefaultAsync(cancellationToken);
        }

        public Task UpdateAsync(GuildSettings setting, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Saving settings for guild {GuildID} to database", setting.GuildID);
            ReplaceOptions options = new ReplaceOptions() { IsUpsert = true };
            return this._collection.ReplaceOneAsync(db => db.GuildID == setting.GuildID, setting, options, cancellationToken);
        }
    }
}
