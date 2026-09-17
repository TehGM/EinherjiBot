using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.UI.API
{
    public static class DiscordRoleInfoExtensions
    {
        public static string GetHtmlColorOrDefault(this RoleInfoResponse response, string defaultValue = "var(--mud-palette-text-primary)")
            => response.GetHtmlColor() ?? defaultValue;

        /// <summary>Gets CSS style for text colored with role's color, including gradient role colors.</summary>
        public static string GetHtmlTextStyleOrDefault(this RoleInfoResponse response, string defaultColor = "var(--mud-palette-text-primary)")
        {
            string result = $"color: {response.GetHtmlColorOrDefault(defaultColor)};";
            string gradient = response.GetHtmlGradient();
            if (gradient != null)
                result += $" background-image: {gradient}; -webkit-background-clip: text; background-clip: text; -webkit-text-fill-color: transparent;";
            return result;
        }
    }
}
