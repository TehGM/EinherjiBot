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
            string result = $"#{response.Color:X6}";
            if (alpha >= 1.0)
                return result;

            string alphaHex = ((int)(255 * alpha)).ToString("X2");
            return $"{result}{alphaHex}";
        }
    }
}
