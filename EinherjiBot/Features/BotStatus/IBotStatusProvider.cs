namespace TehGM.EinherjiBot.BotStatus
{
    public interface IBotStatusProvider
    {
        Task<BotStatus> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<BotStatus>> GetAllAsync(CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        Task AddOrUpdateAsync(BotStatus status, CancellationToken cancellationToken = default);
    }
}
