namespace TehGM.EinherjiBot.API
{
    public static class RoleInfoExtensions
    {
        public static bool IsFound(this RoleInfoResponse response)
            => response != null && response.ID != 0;

        public static string GetHtmlColor(this RoleInfoResponse response)
        {
            if (response == null || response.Color == 0)
                return null;
            return $"#{response.Color:X6}";
        }
    }
}
