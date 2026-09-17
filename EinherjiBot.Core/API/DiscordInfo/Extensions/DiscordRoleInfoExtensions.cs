namespace TehGM.EinherjiBot.API
{
    public static class DiscordRoleInfoExtensions
    {
        public static bool IsFound(this RoleInfoResponse response)
            => response != null && response.ID != 0;

        public static string GetHtmlColor(this RoleInfoResponse response, double alpha = 1.0)
        {
            if (response == null || response.Color == 0)
                return null;
            return ToHtmlColor(response.Color, alpha);
        }

        /// <summary>Gets CSS linear-gradient for gradient and holographic roles.</summary>
        /// <returns>CSS gradient value; null if role doesn't use gradient colors.</returns>
        public static string GetHtmlGradient(this RoleInfoResponse response)
        {
            if (response?.IsGradient != true)
                return null;

            string primary = ToHtmlColor(response.Color);
            string secondary = ToHtmlColor(response.SecondaryColor.Value);
            if (response.TertiaryColor == null)
                return $"linear-gradient(to right, {primary}, {secondary}, {primary})";
            return $"linear-gradient(to right, {primary}, {secondary}, {ToHtmlColor(response.TertiaryColor.Value)}, {primary})";
        }

        private static string ToHtmlColor(uint color, double alpha = 1.0)
        {
            string result = $"#{color:X6}";
            if (alpha >= 1.0)
                return result;

            string alphaHex = ((int)(255 * alpha)).ToString("X2");
            return $"{result}{alphaHex}";
        }
    }
}
