namespace TehGM.EinherjiBot.BotStatus
{
    public interface IBotStatusHandler
    {
        Task<IEnumerable<BotStatusResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<BotStatusResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<BotStatusResponse> CreateAsync(BotStatusRequest request, CancellationToken cancellationToken = default);
        Task<EntityUpdateResult<BotStatusResponse>> UpdateAsync(Guid id, BotStatusRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        Task SetCurrentAsync(BotStatusRequest request, CancellationToken cancellationToken = default);
    }
}
