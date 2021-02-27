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
    public class MongoGameServerStore : IGameServerStore, IDisposable
    {
        // caching
        public const string CacheOptionName = "GameServers";
        private readonly IEntityCache<string, GameServer> _cache;
        private readonly IOptionsMonitor<CachingOptions> _cachingOptions;
        // db
        private readonly IMongoConnection _databaseConnection;
        private IMongoCollection<GameServer> _gameServersCollection;
        private readonly IOptionsMonitor<DatabaseOptions> _databaseOptions;
        // misc
        private readonly ILogger _log;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);


        public MongoGameServerStore(IMongoConnection databaseConnection, ILogger<MongoGameServerStore> log, IEntityCache<string, GameServer> cache,
            IOptionsMonitor<DatabaseOptions> databaseOptions, IOptionsMonitor<CachingOptions> cachingOptions)
        {
            this._databaseConnection = databaseConnection;
            this._databaseOptions = databaseOptions;
            this._cache = cache;
            this._cachingOptions = cachingOptions;
            this._log = log;

            this._databaseConnection.ClientChanged += OnClientChanged;
            this.OnClientChanged(this._databaseConnection.Client);
        }

        private void OnClientChanged(MongoClient client)
        {
            this._gameServersCollection = client
                .GetDatabase(this._databaseOptions.CurrentValue.DatabaseName)
                .GetCollection<GameServer>(this._databaseOptions.CurrentValue.GameServersCollectionName);
        }

        public async Task<GameServer> GetAsync(string name, CancellationToken cancellationToken = default)
        {
            string trimmedName = name.Trim();
            string lowercaseName = trimmedName.ToLowerInvariant();
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                GameServer result;
                CachingOptions cachingOptions = _cachingOptions.Get(CacheOptionName);
                if (cachingOptions.Enabled)
                {
                    result = this._cache.Get(lowercaseName);
                    if (result != null)
                    {
                        _log.LogTrace("Server for game {Game} found in cache", trimmedName);
                        return result;
                    }
                }

                // get from DB
                _log.LogTrace("Retrieving server for game {Game} from database", trimmedName);
                result = await this._gameServersCollection.Find(server => server.Game.ToLowerInvariant() == lowercaseName)
                    .FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                // if not found, return null
                if (result == null)
                {
                    _log.LogTrace("Server for {Game} not found", trimmedName);
                    return null;
                }

                this._cache.AddOrReplace(result.Game, result, cachingOptions.Lifetime);
                return result;
            }
            finally
            {
                _lock.Release();
            }
        }

        public void Dispose()
        {
            this._databaseConnection.ClientChanged -= OnClientChanged;
        }
    }
}
