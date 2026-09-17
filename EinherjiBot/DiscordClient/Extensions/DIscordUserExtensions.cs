using Discord;
using Discord.WebSocket;

namespace TehGM.EinherjiBot
{
    public static class DiscordUserExtensions
    {
        public static bool IsOwner(this IGuildUser user)
            => user.Id == user.Guild.OwnerId;

        public static IEnumerable<IRole> GetRoles(this IGuildUser user, Func<IRole, bool> filter = null)
            => user.Guild.Roles.Where(r => 
                user.RoleIds.Contains(r.Id) 
                && (filter?.Invoke(r) ?? true));
        public static IRole GetHighestRole(this IGuildUser user, Func<IRole, bool> filter = null)
            => GetRoles(user, filter)
                .OrderByDescending(r => r.Position)
                .FirstOrDefault();


        public static Color GetUserColour(this IGuildUser user)
            => GetHighestRole(user, role => role.Colors.PrimaryColor != default)?.Colors.PrimaryColor ?? Color.Default;


        public static Task ModifyAsync(this IGuildUser user, Action<GuildUserProperties> func, CancellationToken cancellationToken)
            => user.ModifyAsync(func, cancellationToken.ToRequestOptions());
        public static string GetMaxAvatarUrl(this IUser user, ImageFormat format = ImageFormat.Auto)
            => GetSafeAvatarUrl(user, format, (ushort)(user is SocketUser ? 2048 : 1024));
        public static string GetSafeAvatarUrl(this IUser user, ImageFormat format = ImageFormat.Auto, ushort size = 128)
            => user.GetAvatarUrl(format, size) ?? user.GetDefaultAvatarUrl();

        /// <summary>Gets user's display name if set, or username otherwise.</summary>
        public static string GetDisplayName(this IUser user)
            => string.IsNullOrWhiteSpace(user.GlobalName) ? user.Username : user.GlobalName;
    }
}
