namespace TehGM.EinherjiBot.BotStatus
{
    public interface IStatusService
    {
        Task<Status> RandomizeStatusAsync(CancellationToken cancellationToken = default);
        Task SetStatusAsync(Status status, CancellationToken cancellationToken = default);
    }
}
