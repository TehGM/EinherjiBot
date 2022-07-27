using MongoDB.Driver;
using TehGM.EinherjiBot.Database;

namespace TehGM.EinherjiBot.Security.Services
{
    public class MongoRefreshTokenStore : IRefreshTokenStore
    {
        private readonly IMongoCollection<RefreshToken> _collection;
        private readonly ILogger _log;

        public MongoRefreshTokenStore(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, ILogger<MongoRefreshTokenStore> log)
        {
            this._collection = databaseConnection.GetCollection<RefreshToken>(databaseOptions.CurrentValue.RefreshTokensCollectionName);
            this._log = log;
        }

        public Task<RefreshToken> GetAsync(string token, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Retrieving refresh token {Token} from database", token);
            return this._collection.Find(dbData => dbData.Token == token).FirstOrDefaultAsync(cancellationToken);
        }

        public Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Inserting refresh token {Token} to database", token);
            return this._collection.InsertOneAsync(token, null, cancellationToken);
        }

        public Task DeleteAsync(string token, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Deleting refresh token {Token} from database", token);
            return this._collection.DeleteOneAsync(dbData => dbData.Token == token, cancellationToken);
        }
    }
}
