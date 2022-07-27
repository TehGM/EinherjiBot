using System.Security.Claims;
using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.API;

namespace TehGM.EinherjiBot
{
    public static class AuthContextExtensions
    {
        public static bool IsLoggedIn(this IAuthContext context)
            => context != null && context.ID != default;

        public static bool IsAdmin(this IAuthContext context)
            => context.BotRoles.Contains(UserRole.Admin);

        public static ClaimsPrincipal ToClaimsPrincipal(this IAuthContext context, string scheme)
        {
            if (context.ID == default)
                return new ClaimsPrincipal();

            ClaimsIdentity identity = new ClaimsIdentity(scheme, ClaimNames.UserID, ClaimNames.Roles);
            identity.AddClaim(new Claim(ClaimNames.UserID, context.ID.ToString()));
            foreach (string role in context.BotRoles)
                identity.AddClaim(new Claim(ClaimNames.Roles, role));
            return new ClaimsPrincipal(identity);
        }
    }
}
