using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Database;
using TehGM.EinherjiBot.Database.Services;

namespace TehGM.EinherjiBot.Patchbot.Services
{
    public class MongoPatchbotGameStore : MongoBatchingRepositoryBase<string, PatchbotGame>, IBatchingRepository, IPatchbotGamesStore, IDisposable
    {
        public const string CacheOptionName = "PatchbotGames";
        private readonly IEntityCache<string, PatchbotGame> _patchbotGameCache;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<CachingOptions> _cachingOptions;
        private IMongoCollection<PatchbotGame> _collection;
        private readonly ReplaceOptions _replaceOptions = new ReplaceOptions() { IsUpsert = true };
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public MongoPatchbotGameStore(IMongoConnection databaseConnection, IOptionsMonitor<DatabaseOptions> databaseOptions, IHostApplicationLifetime hostLifetime, ILogger<MongoPatchbotGameStore> log, IEntityCache<string, PatchbotGame> patchbotGameCache, IOptionsMonitor<CachingOptions> cachingOptions)
            : base(databaseConnection, databaseOptions, hostLifetime, log)
        {
            this._patchbotGameCache = patchbotGameCache;
            this._log = log;
            this._cachingOptions = cachingOptions;

            base.MongoConnection.ClientChanged += OnClientChanged;
            OnClientChanged(base.MongoConnection.Client);
        }

        private void OnClientChanged(MongoClient client)
        {
            DatabaseOptions options = base.DatabaseOptions.CurrentValue;
            this._collection = base.MongoConnection.Client
                .GetDatabase(options.DatabaseName)
                .GetCollection<PatchbotGame>(options.PatchbotGamesCollectionName);
            base.RecreateBatchInserter(_cachingOptions.Get(CacheOptionName).Lifetime, this._collection);
        }

        public async Task<PatchbotGame> GetAsync(string name, CancellationToken cancellationToken = default)
        {
            string trimmedName = name.Trim();
            string lowercaseName = trimmedName.ToLowerInvariant();
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                PatchbotGame result;
                CachingOptions cachingOptions = _cachingOptions.Get(CacheOptionName);
                if (cachingOptions.Enabled)
                {
                    result = _patchbotGameCache.Find(e => e.Entity.MatchesName(trimmedName)).FirstOrDefault();
                    if (result != null)
                    {
                        _log.LogTrace("Patchbot game {Game} found in cache", trimmedName);
                        return result;
                    }
                }

                // get from DB
                _log.LogTrace("Retrieving patchbot game {Game} from database", trimmedName);
                FilterDefinition<PatchbotGame> filter = Builders<PatchbotGame>.Filter.Or(
                    Builders<PatchbotGame>.Filter.Eq(dbData => dbData.Name.ToLowerInvariant(), lowercaseName),
                    Builders<PatchbotGame>.Filter.AnyEq(dbData => dbData.Aliases, lowercaseName));
                result = await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                // if not found, return null
                if (result == null)
                {
                    _log.LogTrace("Patchbot game {Game} not found", trimmedName);
                    return null;
                }

                _patchbotGameCache.AddOrReplace(result.Name, result, cachingOptions.Lifetime);
                return result;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SetAsync(PatchbotGame game, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _log.LogTrace("Inserting patchbot game {Game} into next DB batch", game.Name);
                _patchbotGameCache.AddOrReplace(game.Name, game, _cachingOptions.Get(CacheOptionName).Lifetime);
                await base.BatchInserter.BatchAsync(game.Name, new MongoDelayedInsert<PatchbotGame>(dbData => dbData.Name == game.Name, game, _replaceOptions), cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task DeleteAsync(PatchbotGame game, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _log.LogTrace("Inserting patchbot game {Game} into next DB batch", game.Name);
                await base.BatchInserter.UnbatchAsync(game.Name, cancellationToken).ConfigureAwait(false);
                _patchbotGameCache.Remove(game.Name);
                await _collection.DeleteOneAsync(dbData => dbData.Name == game.Name, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _lock.Release();
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            try { base.MongoConnection.ClientChanged -= OnClientChanged; } catch { }
        }
    }
}
