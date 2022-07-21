using MongoDB.Driver;
using TehGM.EinherjiBot.Database;

namespace TehGM.EinherjiBot.GameServers.Services
{
    public class MongoGameServerStore : IGameServerStore
    {
        private readonly IMongoCollection<GameServer> _collection;
        private readonly ILogger _log;

        public MongoGameServerStore(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, ILogger<MongoGameServerStore> log)
        {
            this._log = log;
            this._collection = databaseConnection.GetCollection<GameServer>(databaseOptions.CurrentValue.GameServersCollectionName);
        }

        public Task<GameServer> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Retrieving game server {ServerID} from database", id);
            return this._collection.Find(db => db.ID == id).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IEnumerable<GameServer>> FindAsync(bool? isPublic, ulong? userID, IEnumerable<ulong> roleIDs, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Retrieving game servers from database; UserID = {UserID}; RoleIDs = {RoleIDs}", userID, string.Join(',', roleIDs ?? Enumerable.Empty<ulong>()));
            List<FilterDefinition<GameServer>> filters = new List<FilterDefinition<GameServer>>(4);
            filters.Add(Builders<GameServer>.Filter.Empty);
            if (isPublic != null)
                filters.Add(Builders<GameServer>.Filter.Eq(db => db.IsPublic, isPublic.Value));
            if (userID != null)
                filters.Add(Builders<GameServer>.Filter.AnyIn(db => db.AuthorizedUserIDs, new[] { userID.Value }));
            if (roleIDs?.Any() == true)
                filters.Add(Builders<GameServer>.Filter.AnyIn(db => db.AuthorizedRoleIDs, roleIDs));
            return await this._collection.Find(Builders<GameServer>.Filter.Or(filters)).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task UpdateAsync(GameServer server, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Saving game server {ServerID} to database", server.ID);
            ReplaceOptions options = new ReplaceOptions() { IsUpsert = true };
            return this._collection.ReplaceOneAsync(db => db.ID == server.ID, server, options, cancellationToken);
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            this._log.LogTrace("Deleting game server {ServerID} from database", id);
            return this._collection.DeleteOneAsync(db => db.ID == id, cancellationToken);
        }
    }
}
