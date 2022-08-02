namespace TehGM.EinherjiBot.Security.Authorization.Policies
{
    public class AuthorizeAttribute : DiscordAuthorizationAttribute
    {
        public AuthorizeAttribute() : base(typeof(Authorize)) { }
    }
}
