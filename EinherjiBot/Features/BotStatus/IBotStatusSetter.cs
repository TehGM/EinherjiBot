namespace TehGM.EinherjiBot.BotStatus
{
    public interface IBotStatusSetter
    {
        Task<Status> RandomizeStatusAsync(CancellationToken cancellationToken = default);
        Task SetStatusAsync(Status status, CancellationToken cancellationToken = default);
    }
}
