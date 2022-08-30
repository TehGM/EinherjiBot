using TehGM.EinherjiBot.PlaceholdersEngine;

namespace TehGM.EinherjiBot.UI.PlaceholdersEngine
{
    public interface IPlaceholdersBuilder
    {
        Task<PlaceholderBuilderResult> OpenAsync(PlaceholderConvertContext context, bool allowAdminContext = true);
    }
}
