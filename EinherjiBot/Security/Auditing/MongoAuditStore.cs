using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using TehGM.EinherjiBot.Database;
using TehGM.EinherjiBot.Database.Services;

namespace TehGM.EinherjiBot.Auditing.Services
{
    public class MongoAuditStore<TAudit> : MongoBatchingRepositoryBase<Guid, TAudit>, IAuditStore<TAudit> where TAudit : BotAuditEntry
    {
        protected override IMongoCollection<TAudit> Collection => base.MongoConnection
            .GetCollection<TAudit>(base.DatabaseOptions.CurrentValue.AuditCollectionName);
        protected override TimeSpan BatchDelay => TimeSpan.FromMinutes(5);

        public MongoAuditStore(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, IHostApplicationLifetime hostLifetime, ILogger<MongoAuditStore<TAudit>> log)
            : base(databaseConnection, databaseOptions, hostLifetime, log) { }

        public Task AddAuditAsync(TAudit audit, CancellationToken cancellationToken = default)
            => base.BatchInserter.BatchAsync(audit.ID, new MongoDelayedInsert<TAudit>(audit), cancellationToken);

        public async Task<IEnumerable<TAudit>> FindAuditsAsync(FilterDefinition<TAudit> filter, CancellationToken cancellationToken = default)
        {
            // since audits are not cached in-memory, there's no layer that would prevent ignoring batched inserts
            // for this reason we need to flush batch first
            await base.FlushBatchAsync().ConfigureAwait(false);
            return await this.Collection.Find(filter).ToListAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task<TAudit> GetAuditAsync(Guid id, CancellationToken cancellationToken = default)
        {
            // since audits are not cached in-memory, there's no layer that would prevent ignoring batched inserts
            // for this reason we need to flush batch first
            await base.FlushBatchAsync().ConfigureAwait(false);
            return await this.Collection.Find(Builders<TAudit>.Filter.Eq(audit => audit.ID, id)).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
