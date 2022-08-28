namespace TehGM.EinherjiBot.BotStatus
{
    public interface IBotStatusSetter
    {
        Task<BotStatus> RandomizeStatusAsync(CancellationToken cancellationToken = default);
        Task SetStatusAsync(BotStatus status, CancellationToken cancellationToken = default);
    }
}
