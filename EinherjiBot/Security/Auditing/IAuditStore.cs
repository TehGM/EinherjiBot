using MongoDB.Driver;

namespace TehGM.EinherjiBot.Auditing
{
    public interface IAuditStore<TAudit>
    {
        Task AddAuditAsync(TAudit audit, CancellationToken cancellationToken = default);
        Task<IEnumerable<TAudit>> FindAuditsAsync(FilterDefinition<TAudit> filter, CancellationToken cancellationToken = default);
        Task<TAudit> GetAuditAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
