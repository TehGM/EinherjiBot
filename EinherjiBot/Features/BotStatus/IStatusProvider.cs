namespace TehGM.EinherjiBot.BotStatus
{
    public interface IStatusProvider
    {
        Task<Status> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Status>> GetAllAsync(CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        Task UpdateAsync(Status status, CancellationToken cancellationToken = default);
    }
}
