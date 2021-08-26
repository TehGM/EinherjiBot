using DSharpPlus;
using DSharpPlus.Entities;

namespace TehGM.EinherjiBot
{
    public static class DiscordMemberExtensions
    {
        public static bool HasPermissions(this DiscordMember member, Permissions permissions)
            => (member.Permissions & permissions) == permissions;

        public static bool HasChannelPermissions(this DiscordMember member, DiscordChannel channel, Permissions permissions)
            => (member.PermissionsIn(channel) & permissions) == permissions;
    }
}
