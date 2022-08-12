using System.Web;

namespace TehGM.EinherjiBot.API
{
    public static class DiscordGuildInfoExtensions
    {
        public static bool IsFound(this IDiscordGuildInfo response)
            => response != null && response.ID != 0;

        public static string GetIconURL(this IDiscordGuildInfo guild, ushort size = 1024)
        {
            if (string.IsNullOrWhiteSpace(guild.IconHash))
            {
                string encodedName = HttpUtility.UrlEncodeUnicode(guild.Name);
                return $"https://ui-avatars.com/api?name={encodedName}&size={size}&length=3&uppercase=false&format=png";
            }
            string ext = guild.IconHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png";
            return $"https://cdn.discordapp.com/icons/{guild.ID}/{guild.IconHash}.{ext}?size={size}";
        }
    }
}
