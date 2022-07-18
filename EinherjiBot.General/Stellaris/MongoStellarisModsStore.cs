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
        private readonly IEntityCache<ObjectId, StellarisMod> _cache;
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);
        private DateTime _lastCacheTimeUtc;
        // db
        private IMongoCollection<StellarisMod> _collection;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        // misc
        private readonly ILogger _log;

        public MongoStellarisModsStore(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, ILogger<MongoStellarisModsStore> log, IEntityCache<ObjectId, StellarisMod> cache)
        {
            this._log = log;
            this._cache = cache;
            this._cache.DefaultExpiration = new TimeSpanEntityExpiration(_cacheExpiration);

            this._collection = databaseConnection
                .GetCollection<StellarisMod>(databaseOptions.CurrentValue.StellarisModsCollectionName);
        }

        public async Task AddAsync(StellarisMod mod, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this._log.LogDebug("Adding mod {Name} to database", mod.Name);
                await this._collection.InsertOneAsync(mod, null, cancellationToken).ConfigureAwait(false);
                this._cache.AddOrReplace(mod);
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
                if (this._lastCacheTimeUtc + this._cacheExpiration < DateTime.UtcNow)
                {
                    _log.LogTrace("Stellaris mods found in cache");
                    return _cache.Find(_ => true);
                }

                this._log.LogDebug("Retrieving Stellaris mods from database");
                IEnumerable<StellarisMod> results = await this._collection.Find(_ => true).ToListAsync(cancellationToken).ConfigureAwait(false);
                if (results?.Any() != true)
                {
                    this._log.LogTrace("Stellaris mods not found, returning empty");
                    results = Enumerable.Empty<StellarisMod>();
                }

                this._cache.Clear();
                foreach (StellarisMod mod in results)
                    this._cache.AddOrReplace(mod);
                this._lastCacheTimeUtc = DateTime.UtcNow;
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
                this._cache.RemoveWhere(e => ids.Contains(e.ID));
            }
            finally
            {
                this._lock.Release();
            }
        }
    }
}
