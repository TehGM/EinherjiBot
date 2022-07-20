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
        private readonly IEntityCache<string, PatchbotGame> _cache;
        private readonly ILogger _log;
        private readonly ReplaceOptions _replaceOptions = new ReplaceOptions() { IsUpsert = true };
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        protected override TimeSpan BatchDelay => TimeSpan.FromMinutes(10);
        protected override IMongoCollection<PatchbotGame> Collection => base.MongoConnection
                .GetCollection<PatchbotGame>(base.DatabaseOptions.CurrentValue.PatchbotGamesCollectionName);

        public MongoPatchbotGameStore(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, IHostApplicationLifetime hostLifetime, ILogger<MongoPatchbotGameStore> log, IEntityCache<string, PatchbotGame> cache)
            : base(databaseConnection, databaseOptions, hostLifetime, log)
        {
            this._cache = cache;
            this._log = log;
        }

        public async Task<PatchbotGame> GetAsync(string name, CancellationToken cancellationToken = default)
        {
            string trimmedName = name.Trim();
            string lowercaseName = trimmedName.ToLowerInvariant();
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                PatchbotGame result = this._cache.Find(e => e.MatchesName(trimmedName)).FirstOrDefault();
                if (result != null)
                {
                    this._log.LogTrace("Patchbot game {Game} found in cache", trimmedName);
                    return result;
                }

                // get from DB
                _log.LogTrace("Retrieving patchbot game {Game} from database", trimmedName);
                FilterDefinition<PatchbotGame> filter = Builders<PatchbotGame>.Filter.Or(
                    Builders<PatchbotGame>.Filter.Regex(dbData => dbData.Name, new BsonRegularExpression($"/^{trimmedName}$/i")),
                    Builders<PatchbotGame>.Filter.AnyEq(dbData => dbData.Aliases, lowercaseName));
                result = await this.Collection.Find(filter).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                // if not found, return null
                if (result == null)
                    this._log.LogTrace("Patchbot game {Game} not found", trimmedName);

                this._cache.AddOrReplace(result);
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
                this._cache.AddOrReplace(game);
                await base.BatchInserter.BatchAsync(game.Name, new MongoDelayedUpsert<PatchbotGame>(dbData => dbData.Name == game.Name, game, this._replaceOptions), cancellationToken).ConfigureAwait(false);
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
                this._cache.Remove(game);
                await this.Collection.DeleteOneAsync(dbData => dbData.Name == game.Name, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                this._lock.Release();
            }
        }
    }
}
