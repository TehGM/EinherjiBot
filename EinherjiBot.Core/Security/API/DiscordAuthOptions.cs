namespace TehGM.EinherjiBot.Security.API
{
    public class DiscordAuthOptions
    {
        public string ClientID { get; set; }
        public string ClientSecret { get; set; }
        public string[] Scopes { get; set; } = { "identify", "guilds" };
        public string RedirectURL { get; set; } = EinherjiInfo.WebsiteURL + "/login/oauth";
    }
}
