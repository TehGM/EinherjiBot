using MongoDB.Driver;
using TehGM.EinherjiBot.Database;

namespace TehGM.EinherjiBot.Security.Services
{
    public class MongoUserSecurityDataStore : IUserSecurityDataStore
    {
        private readonly IMongoCollection<UserSecurityData> _collection;
        private readonly ILogger _log;
        private readonly ReplaceOptions _replaceOptions;

        public MongoUserSecurityDataStore(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, ILogger<MongoUserSecurityDataStore> log)
        {
            this._collection = databaseConnection.GetCollection<UserSecurityData>(databaseOptions.CurrentValue.UserDataCollectionName);
            this._log = log;
            this._replaceOptions = new ReplaceOptions() { IsUpsert = true, BypassDocumentValidation = false };
        }

        public Task<UserSecurityData> GetAsync(ulong userID, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Retrieving user security data for user {UserID} from database", userID);
            return this._collection.Find(dbData => dbData.ID == userID)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task UpdateAsync(UserSecurityData data, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Inserting user security data for user {UserID} into  DB", data.ID);
            return this._collection.ReplaceOneAsync(dbData => dbData.ID == data.ID, data, this._replaceOptions, cancellationToken);
        }
    }
}
