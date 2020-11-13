using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Database;

namespace TehGM.EinherjiBot.Stellaris.Services
{
    public class MongoStellarisModsStore : IStellarisModsStore, IDisposable
    {
        // caching
        public const string CacheOptionName = "StellarisMods";
        private readonly IEntityCache<ObjectId, StellarisMod> _stellarisModsCache;
        private readonly IOptionsMonitor<CachingOptions> _cachingOptions;
        private DateTime _lastCacheTimeUtc;
        // db
        private readonly IMongoConnection _databaseConnection;
        private IMongoCollection<StellarisMod> _collection;
        private readonly IOptionsMonitor<DatabaseOptions> _databaseOptions;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        // misc
        private readonly ILogger _log;

        public MongoStellarisModsStore(IMongoConnection databaseConnection, IOptionsMonitor<DatabaseOptions> databaseOptions, IOptionsMonitor<CachingOptions> cachingOptions, ILogger<MongoStellarisModsStore> log, IEntityCache<ObjectId, StellarisMod> stellarisModsCache)
        {
            this._databaseConnection = databaseConnection;
            this._databaseOptions = databaseOptions;
            this._cachingOptions = cachingOptions;
            this._log = log;
            this._stellarisModsCache = stellarisModsCache;

            this._databaseConnection.ClientChanged += OnClientChanged;
        }

        private void OnClientChanged(MongoClient client)
        {
            DatabaseOptions options = this._databaseOptions.CurrentValue;
            this._collection = this._databaseConnection.Client
                .GetDatabase(options.DatabaseName)
                .GetCollection<StellarisMod>(options.StellarisModsCollectionName);
        }

        public async Task AddAsync(StellarisMod mod, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _log.LogDebug("Adding mod {Name} to database", mod.Name);
                await _collection.InsertOneAsync(mod, null, cancellationToken).ConfigureAwait(false);
                if (_cachingOptions.CurrentValue.Enabled)
                    _stellarisModsCache.AddOrReplace(mod.ID, mod, _cachingOptions.CurrentValue.Lifetime);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<IEnumerable<StellarisMod>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                CachingOptions cachingOptions = _cachingOptions.Get(CacheOptionName);
                if (cachingOptions.Enabled && _lastCacheTimeUtc + cachingOptions.Lifetime < DateTime.UtcNow)
                {
                    _log.LogTrace("Stellaris mods found in cache");
                    return _stellarisModsCache.Find(_ => true).Select(e => e.Entity);
                }

                _log.LogDebug("Retrieving Stellaris mods from database");
                IEnumerable<StellarisMod> results = await _collection.Find(_ => true).ToListAsync(cancellationToken).ConfigureAwait(false);
                if (results?.Any() != true)
                {
                    _log.LogTrace("Stellaris mods not found, returning empty");
                    results = Enumerable.Empty<StellarisMod>();
                }

                if (cachingOptions.Enabled)
                {
                    _stellarisModsCache.Clear();
                    foreach (StellarisMod mod in results)
                        _stellarisModsCache.AddOrReplace(mod.ID, mod, cachingOptions.Lifetime);
                }
                return results;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task RemoveAsync(IEnumerable<StellarisMod> mods, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _log.LogDebug("Removing {Count} Stellaris mods from the database", mods.Count());
                IEnumerable<ObjectId> ids = mods.Select(e => e.ID);
                FilterDefinition<StellarisMod> filter = Builders<StellarisMod>.Filter.In(e => e.ID, ids);
                await _collection.DeleteManyAsync(filter, cancellationToken).ConfigureAwait(false);
                if (_cachingOptions.CurrentValue.Enabled)
                    _stellarisModsCache.RemoveWhere(e => ids.Contains(e.Entity.ID));
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
