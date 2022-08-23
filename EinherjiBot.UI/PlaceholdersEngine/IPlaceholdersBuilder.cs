using TehGM.EinherjiBot.PlaceholdersEngine;

namespace TehGM.EinherjiBot.UI.PlaceholdersEngine
{
    public interface IPlaceholdersBuilder
    {
        Task<PlaceholderBuilderResult> OpenAsync(PlaceholderUsage context, bool allowAdminContext = true);
    }
}
