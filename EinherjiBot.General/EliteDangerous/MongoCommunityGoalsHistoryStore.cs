using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.Database;
using TehGM.EinherjiBot.Database.Services;

namespace TehGM.EinherjiBot.EliteDangerous.Services
{
    public class MongoCommunityGoalsHistoryStore : MongoBatchingRepositoryBase<int, CommunityGoal>, IBatchingRepository, ICommunityGoalsHistoryStore, IDisposable
    {
        public const string CacheOptionName = "EliteCommunityGoals";
        private readonly IEntityCache<int, CommunityGoal> _cgCache;
        private readonly ILogger _log;
        private readonly IOptionsMonitor<CachingOptions> _cachingOptions;
        private IMongoCollection<CommunityGoal> _collection;
        private readonly ReplaceOptions _replaceOptions = new ReplaceOptions() { IsUpsert = true };
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public MongoCommunityGoalsHistoryStore(IMongoConnection databaseConnection, IOptionsMonitor<DatabaseOptions> databaseOptions, IHostApplicationLifetime hostLifetime, ILogger<MongoCommunityGoalsHistoryStore> log, IEntityCache<int, CommunityGoal> cgCache, IOptionsMonitor<CachingOptions> cachingOptions)
            : base(databaseConnection, databaseOptions, hostLifetime, log)
        {
            this._cgCache = cgCache;
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
                .GetCollection<CommunityGoal>(options.EliteCommunityGoalsCollectionName);
            base.RecreateBatchInserter(_cachingOptions.Get(CacheOptionName).Lifetime, this._collection);
        }

        public async Task<CommunityGoal> GetAsync(int id, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                CommunityGoal result;
                CachingOptions cachingOptions = _cachingOptions.Get(CacheOptionName);
                if (cachingOptions.Enabled && (result = _cgCache.Get(id)) != null)
                {
                    _log.LogTrace("Community goal {ID} ({Name}) found in cache", id, result.Name);
                    return result;
                }

                // get from DB
                _log.LogTrace("Retrieving community goal {ID} from history database", id);
                result = await _collection.Find(dbData => dbData.ID == id).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);

                if (result != null)
                    _cgCache.AddOrReplace(result.ID, result, cachingOptions.Lifetime);
                return result;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task SetAsync(CommunityGoal cg, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                _log.LogTrace("Inserting community goal {ID} history entry into next DB batch", cg.ID);
                _cgCache.AddOrReplace(cg.ID, cg, _cachingOptions.Get(CacheOptionName).Lifetime);
                await base.BatchInserter.BatchAsync(cg.ID, new MongoDelayedInsert<CommunityGoal>(dbData => dbData.ID == cg.ID, cg, _replaceOptions), cancellationToken).ConfigureAwait(false);
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
