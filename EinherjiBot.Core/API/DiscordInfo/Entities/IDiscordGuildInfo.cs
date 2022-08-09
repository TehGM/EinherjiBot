using System.Web;

namespace TehGM.EinherjiBot.API
{
    public interface IDiscordGuildInfo : IDiscordEntityInfo
    {
        new ulong ID { get; }
        new string Name { get; }
        string IconHash { get; }

        ulong IDiscordEntityInfo.ID => this.ID;
        string IDiscordEntityInfo.Name => this.Name;

        string GetIconURL(ushort size = 1024)
        {
            if (string.IsNullOrWhiteSpace(this.IconHash))
            {
                string encodedName = HttpUtility.UrlEncodeUnicode(this.Name);
                return $"https://ui-avatars.com/api?name={encodedName}&size={size}&length=3&uppercase=false&format=png";
            }
            string ext = this.IconHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png";
            return $"https://cdn.discordapp.com/icons/{this.ID}/{this.IconHash}.{ext}?size={size}";
        }
    }
}
