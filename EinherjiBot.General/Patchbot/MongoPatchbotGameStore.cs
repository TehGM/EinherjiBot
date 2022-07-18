using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Database;
using TehGM.EinherjiBot.Database.Services;

namespace TehGM.EinherjiBot.Patchbot.Services
{
    public class MongoPatchbotGameStore : MongoBatchingRepositoryBase<string, PatchbotGame>, IBatchingRepository, IPatchbotGamesStore
    {
        public const string CacheOptionName = "PatchbotGames";
        private readonly IEntityCache<string, PatchbotGame> _patchbotGameCache;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<CachingOptions> _cachingOptions;
        private IMongoCollection<PatchbotGame> _collection;
        private readonly ReplaceOptions _replaceOptions = new ReplaceOptions() { IsUpsert = true };
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        protected override TimeSpan BatchDelay => TimeSpan.FromMinutes(10);
        protected override IMongoCollection<PatchbotGame> Collection => base.MongoConnection
                .GetCollection<PatchbotGame>(base.DatabaseOptions.CurrentValue.PatchbotGamesCollectionName);

        public MongoPatchbotGameStore(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, IHostApplicationLifetime hostLifetime, ILogger<MongoPatchbotGameStore> log, IEntityCache<string, PatchbotGame> patchbotGameCache, IOptionsMonitor<CachingOptions> cachingOptions)
            : base(databaseConnection, databaseOptions, hostLifetime, log)
        {
            this._patchbotGameCache = patchbotGameCache;
            this._log = log;
            this._cachingOptions = cachingOptions;
        }

        public async Task<PatchbotGame> GetAsync(string name, CancellationToken cancellationToken = default)
        {
            string trimmedName = name.Trim();
            string lowercaseName = trimmedName.ToLowerInvariant();
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                PatchbotGame result;
                CachingOptions cachingOptions = this._cachingOptions.Get(CacheOptionName);
                if (cachingOptions.Enabled)
                {
                    result = this._patchbotGameCache.Find(e => e.Entity.MatchesName(trimmedName)).FirstOrDefault();
                    if (result != null)
                    {
                        this._log.LogTrace("Patchbot game {Game} found in cache", trimmedName);
                        return result;
                    }
                }

                // get from DB
                _log.LogTrace("Retrieving patchbot game {Game} from database", trimmedName);
                FilterDefinition<PatchbotGame> filter = Builders<PatchbotGame>.Filter.Or(
                    Builders<PatchbotGame>.Filter.Regex(dbData => dbData.Name, new BsonRegularExpression($"/^{trimmedName}$/i")),
                    Builders<PatchbotGame>.Filter.AnyEq(dbData => dbData.Aliases, lowercaseName));
                result = await this._collection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                // if not found, return null
                if (result == null)
                {
                    this._log.LogTrace("Patchbot game {Game} not found", trimmedName);
                    return null;
                }

                this._patchbotGameCache.AddOrReplace(result.Name, result, cachingOptions.Lifetime);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task UpdateAsync(PatchbotGame game, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this._log.LogTrace("Inserting patchbot game {Game} into next DB batch", game.Name);
                this._patchbotGameCache.AddOrReplace(game.Name, game, this._cachingOptions.Get(CacheOptionName).Lifetime);
                await base.BatchInserter.BatchAsync(game.Name, new MongoDelayedInsert<PatchbotGame>(dbData => dbData.Name == game.Name, game, this._replaceOptions), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task DeleteAsync(PatchbotGame game, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this._log.LogTrace("Inserting patchbot game {Game} into next DB batch", game.Name);
                await base.BatchInserter.UnbatchAsync(game.Name, cancellationToken).ConfigureAwait(false);
                this._patchbotGameCache.Remove(game.Name);
                await this._collection.DeleteOneAsync(dbData => dbData.Name == game.Name, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                this._lock.Release();
            }
        }
    }
}
