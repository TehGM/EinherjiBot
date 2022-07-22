namespace TehGM.EinherjiBot.Security
{
    public static class AuthContextExtensions
    {
        public static bool IsAdmin(this IAuthContext userContext)
            => userContext.BotRoles.Contains(UserRole.Admin);
    }
}
