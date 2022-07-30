namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    public interface IPlaceholder
    {
        Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default);
    }
}
