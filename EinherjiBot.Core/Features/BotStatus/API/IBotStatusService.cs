namespace TehGM.EinherjiBot.BotStatus.API
{
    public interface IBotStatusService
    {
        Task<IEnumerable<BotStatusResponse>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<BotStatusResponse> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<BotStatusResponse> CreateAsync(BotStatusRequest request, CancellationToken cancellationToken = default);
        Task<BotStatusResponse> UpdateAsync(Guid id, BotStatusRequest request, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        Task SetCurrentAsync(BotStatusRequest request, CancellationToken cancellationToken = default);
    }
}
