using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.UI.API
{
    public static class DiscordRoleInfoExtensions
    {
        public static string GetHtmlColorOrDefault(this RoleInfoResponse response, string defaultValue = "var(--mud-palette-text-primary)")
            => response.GetHtmlColor() ?? defaultValue;
    }
}
