namespace TehGM.EinherjiBot.Security.Policies
{
    public class AuthorizeBotOrAdminAttribute : DiscordAuthorizationAttribute
    {
        public AuthorizeBotOrAdminAttribute() : base(typeof(AuthorizeBotOrAdmin)) { }
    }
}
