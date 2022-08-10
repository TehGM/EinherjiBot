namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    public interface IPlaceholderHandler
    {
        Task<string> GetReplacementAsync(object placeholder, CancellationToken cancellationToken = default);
    }
}
