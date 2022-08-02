namespace TehGM.EinherjiBot.Security.Authorization.Policies
{
    public class AuthorizeAdminAttribute : DiscordAuthorizationAttribute
    {
        public AuthorizeAdminAttribute() : base(typeof(AuthorizeAdmin)) { }
    }
}
