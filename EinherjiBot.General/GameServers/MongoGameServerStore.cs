using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Database;

namespace TehGM.EinherjiBot.GameServers.Services
{
    public class MongoGameServerStore : IGameServerStore
    {
        // caching
        private readonly IEntityCache<string, GameServer> _cache;
        // db
        private IMongoCollection<GameServer> _gameServersCollection;
        private readonly IOptionsMonitor<MongoOptions> _databaseOptions;
        // misc
        private readonly ILogger _log;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);


        public MongoGameServerStore(IMongoConnection databaseConnection, ILogger<MongoGameServerStore> log, IEntityCache<string, GameServer> cache,
            IOptionsMonitor<MongoOptions> databaseOptions)
        {
            this._databaseOptions = databaseOptions;
            this._cache = cache;
            this._log = log;
            this._cache.DefaultExpiration = new TimeSpanEntityExpiration(TimeSpan.FromHours(1));

            this._gameServersCollection = databaseConnection
                .GetCollection<GameServer>(this._databaseOptions.CurrentValue.GameServersCollectionName);
        }

        public async Task<GameServer> GetAsync(string name, CancellationToken cancellationToken = default)
        {
            string trimmedName = name.Trim();
            string lowercaseName = trimmedName.ToLowerInvariant();
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                GameServer result = this._cache.Get(lowercaseName);
                if (result != null)
                {
                    this._log.LogTrace("Server for game {Game} found in cache", trimmedName);
                    return result;
                }

                // get from DB
                this._log.LogTrace("Retrieving server for game {Game} from database", trimmedName);
                result = await this._gameServersCollection.Find(server => server.Game.ToLowerInvariant() == lowercaseName)
                    .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                // if not found, return null
                if (result == null)
                {
                    this._log.LogTrace("Server for {Game} not found", trimmedName);
                    return null;
                }

                this._cache.AddOrReplace(result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }
    }
}
