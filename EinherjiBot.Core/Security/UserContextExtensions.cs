namespace TehGM.EinherjiBot.Security
{
    public static class UserContextExtensions
    {
        public static bool IsAdmin(this IUserContext userContext)
            => userContext.Roles.Contains(UserRole.Admin);
    }
}
