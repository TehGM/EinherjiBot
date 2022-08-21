using TehGM.EinherjiBot.PlaceholdersEngine;

namespace TehGM.EinherjiBot.UI.PlaceholdersEngine
{
    public interface IPlaceholdersBuilder
    {
        Task<string> OpenAsync(PlaceholderUsage context);
    }
}
