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
    public class MongoStellarisModsStore : IStellarisModsStore
    {
        // caching
        public const string CacheOptionName = "StellarisMods";
        private readonly IEntityCache<ObjectId, StellarisMod> _stellarisModsCache;
        private readonly IOptionsMonitor<CachingOptions> _cachingOptions;
        private DateTime _lastCacheTimeUtc;
        // db
        private readonly IMongoConnection _databaseConnection;
        private IMongoCollection<StellarisMod> _collection;
        private readonly IOptionsMonitor<MongoOptions> _databaseOptions;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        // misc
        private readonly ILogger _log;

        public MongoStellarisModsStore(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, IOptionsMonitor<CachingOptions> cachingOptions, ILogger<MongoStellarisModsStore> log, IEntityCache<ObjectId, StellarisMod> stellarisModsCache)
        {
            this._databaseConnection = databaseConnection;
            this._databaseOptions = databaseOptions;
            this._cachingOptions = cachingOptions;
            this._log = log;
            this._stellarisModsCache = stellarisModsCache;

            this._collection = databaseConnection
                .GetCollection<StellarisMod>(databaseOptions.CurrentValue.StellarisModsCollectionName);
        }

        public async Task AddAsync(StellarisMod mod, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _log.LogDebug("Adding mod {Name} to database", mod.Name);
                await _collection.InsertOneAsync(mod, null, cancellationToken).ConfigureAwait(false);
                if (this._cachingOptions.CurrentValue.Enabled)
                    this._stellarisModsCache.AddOrReplace(mod.ID, mod, this._cachingOptions.CurrentValue.Lifetime);
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task<IEnumerable<StellarisMod>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                CachingOptions cachingOptions = this._cachingOptions.Get(CacheOptionName);
                if (cachingOptions.Enabled && this._lastCacheTimeUtc + cachingOptions.Lifetime < DateTime.UtcNow)
                {
                    _log.LogTrace("Stellaris mods found in cache");
                    return _stellarisModsCache.Find(_ => true).Select(e => e.Entity);
                }

                this._log.LogDebug("Retrieving Stellaris mods from database");
                IEnumerable<StellarisMod> results = await this._collection.Find(_ => true).ToListAsync(cancellationToken).ConfigureAwait(false);
                if (results?.Any() != true)
                {
                    this._log.LogTrace("Stellaris mods not found, returning empty");
                    results = Enumerable.Empty<StellarisMod>();
                }

                if (cachingOptions.Enabled)
                {
                    this._stellarisModsCache.Clear();
                    foreach (StellarisMod mod in results)
                        this._stellarisModsCache.AddOrReplace(mod.ID, mod, cachingOptions.Lifetime);
                }
                return results;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task RemoveAsync(IEnumerable<StellarisMod> mods, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this._log.LogDebug("Removing {Count} Stellaris mods from the database", mods.Count());
                IEnumerable<ObjectId> ids = mods.Select(e => e.ID);
                FilterDefinition<StellarisMod> filter = Builders<StellarisMod>.Filter.In(e => e.ID, ids);
                await this._collection.DeleteManyAsync(filter, cancellationToken).ConfigureAwait(false);
                if (this._cachingOptions.CurrentValue.Enabled)
                    this._stellarisModsCache.RemoveWhere(e => ids.Contains(e.Entity.ID));
            }
            finally
            {
                this._lock.Release();
            }
        }
    }
}
