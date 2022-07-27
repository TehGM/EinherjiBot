namespace TehGM.EinherjiBot.Security
{
    public static class AuthContextExtensions
    {
        public static bool IsAdmin(this IAuthContext context)
            => context.BotRoles.Contains(UserRole.Admin);
    }
}
