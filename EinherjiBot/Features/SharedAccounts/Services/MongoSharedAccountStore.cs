using MongoDB.Driver;
using TehGM.EinherjiBot.Database;

namespace TehGM.EinherjiBot.SharedAccounts.Services
{
    public class MongoSharedAccountStore : ISharedAccountStore
    {
        private readonly IMongoCollection<SharedAccount> _collection;
        private readonly ILogger _log;

        public MongoSharedAccountStore(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, ILogger<MongoSharedAccountStore> log)
        {
            this._log = log;
            this._collection = databaseConnection.GetCollection<SharedAccount>(databaseOptions.CurrentValue.SharedAccountsCollectionName);
        }

        public Task<SharedAccount> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Retrieving shared account {AccountID} from database", id);
            return this._collection.Find(db => db.ID == id).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<SharedAccount>> FindAsync(SharedAccountType? type, ulong? userID, IEnumerable<ulong> roleIDs, bool forModeration, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Retrieving shared accounts from database; Type = {Type}; UserID = {UserID}; RoleIDs = {RoleIDs}", type, userID, string.Join(',', roleIDs ?? Enumerable.Empty<ulong>()));
            List<FilterDefinition<SharedAccount>> filters = new List<FilterDefinition<SharedAccount>>(5);
            filters.Add(Builders<SharedAccount>.Filter.Empty);
            if (type != null)
                filters.Add(Builders<SharedAccount>.Filter.Eq(db => db.AccountType, type.Value));
            if (userID != null)
                filters.Add(Builders<SharedAccount>.Filter.AnyIn(db => db.AuthorizedUserIDs, new[] { userID.Value }));
            if (roleIDs?.Any() == true)
                filters.Add(Builders<SharedAccount>.Filter.AnyIn(db => db.AuthorizedRoleIDs, roleIDs));
            if (forModeration)
                filters.Add(Builders<SharedAccount>.Filter.AnyIn(db => db.ModUserIDs, new[] { userID.Value }));
            return await this._collection.Find(Builders<SharedAccount>.Filter.And(filters)).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task UpdateAsync(SharedAccount account, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Saving shared account {AccountID} to database", account.ID);
            ReplaceOptions options = new ReplaceOptions() { IsUpsert = true };
            return this._collection.ReplaceOneAsync(db => db.ID == account.ID, account, options, cancellationToken);
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Deleting shared account {AccountID} from database", id);
            return this._collection.DeleteOneAsync(db => db.ID == id, cancellationToken);
        }
    }
}
