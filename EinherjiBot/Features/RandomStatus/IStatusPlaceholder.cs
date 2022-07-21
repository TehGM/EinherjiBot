namespace TehGM.EinherjiBot.RandomStatus.Placeholders
{
    public interface IStatusPlaceholder
    {
        Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default);
    }
}
