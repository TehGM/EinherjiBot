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

        public async Task<IEnumerable<SharedAccount>> FindAsync(SharedAccountFilter filter, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Retrieving shared accounts from database");
            
            return await this._collection.Find(ToMongoFilter(filter)).ToListAsync(cancellationToken).ConfigureAwait(false);
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

        public static FilterDefinition<SharedAccount> ToMongoFilter(SharedAccountFilter filter)
        {
            List<FilterDefinition<SharedAccount>> filters = new List<FilterDefinition<SharedAccount>>(6);
            filters.Add(Builders<SharedAccount>.Filter.Empty);

            if (!string.IsNullOrEmpty(filter.LoginStartsWith))
                filters.Add(Builders<SharedAccount>.Filter.Regex(account => account.Login, new Regex($@"^{filter.LoginStartsWith}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)));
            if (!string.IsNullOrEmpty(filter.LoginContains))
                filters.Add(Builders<SharedAccount>.Filter.Regex(account => account.Login, new Regex(filter.LoginContains, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)));
            if (filter.AccountType != null)
                filters.Add(Builders<SharedAccount>.Filter.Eq(db => db.AccountType, filter.AccountType.Value));

            if (filter.ModUserID != null)
                filters.Add(Builders<SharedAccount>.Filter.AnyIn(db => db.ModUserIDs, new[] { filter.ModUserID.Value }));

            List<FilterDefinition<SharedAccount>> aclFilters = new List<FilterDefinition<SharedAccount>>(2);
            if (filter.AuthorizeUserID != null)
                aclFilters.Add(Builders<SharedAccount>.Filter.AnyIn(db => db.AuthorizedUserIDs, new[] { filter.AuthorizeUserID.Value }));
            if (filter.AuthorizeRoleIDs != null)
                aclFilters.Add(Builders<SharedAccount>.Filter.AnyIn(db => db.AuthorizedRoleIDs, filter.AuthorizeRoleIDs));
            if (aclFilters.Any())
                filters.Add(Builders<SharedAccount>.Filter.Or(aclFilters));

            return Builders<SharedAccount>.Filter.And(filters);
        }
    }
}
