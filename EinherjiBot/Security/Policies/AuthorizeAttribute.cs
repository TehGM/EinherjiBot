using TehGM.EinherjiBot.Security.Authorization;

namespace TehGM.EinherjiBot.Security.Policies
{
    public class AuthorizeAttribute : DiscordAuthorizationAttribute
    {
        public AuthorizeAttribute() : base(typeof(Authorize)) { }
    }
}
