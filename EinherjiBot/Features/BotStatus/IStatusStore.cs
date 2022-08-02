namespace TehGM.EinherjiBot.BotStatus
{
    public interface IStatusStore
    {
        Task<Status> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Status>> GetAllAsync(CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        Task UpsertAsync(Status status, CancellationToken cancellationToken = default);
    }
}
