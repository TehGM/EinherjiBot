using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.API
{
    public static class DiscordEntityInfoServiceExtensions
    {
        /// <summary>Gets Role entity info for one specific role.</summary>
        /// <param name="roleID">ID of the role.</param>
        /// <param name="cancellationToken">Token for operation cancellation.</param>
        /// <returns>Role info if found; otherwise null.</returns>
        public static Task<RoleInfoResponse> GetRoleInfoAsync(this IDiscordEntityInfoService service, ulong roleID, CancellationToken cancellationToken = default)
            => service.GetRoleInfoAsync(roleID, null, cancellationToken);

        /// <summary>Scans for data on guilds that both user and bot are present in.</summary>
        /// <remarks>Due to privacy concerns, non-admin users will only be able to get info on guilds they're present in.</remarks>
        /// <param name="cancellationToken">Token for operation cancellation.</param>
        /// <returns>All Guild infos found.</returns>
        /// <exception cref="AccessForbiddenException">User has no access to requested guild.</exception>
        public static Task<IEnumerable<GuildInfoResponse>> GetGuildInfosAsync(this IDiscordEntityInfoService service, CancellationToken cancellationToken = default)
            => service.GetGuildInfosAsync(null, cancellationToken);

        /// <summary>Scans for data on guilds that both user and bot are present in.</summary>
        /// <remarks>Due to privacy concerns, non-admin users will only be able to get info on guilds they're present in.</remarks>
        /// <param name="id">ID of the guild to check..</param>
        /// <param name="cancellationToken">Token for operation cancellation.</param>
        /// <returns>Guild info if found; otherwise null.</returns>
        /// <exception cref="AccessForbiddenException">User has no access to requested guild.</exception>
        public static async Task<GuildInfoResponse> GetGuildInfoAsync(this IDiscordEntityInfoService service, ulong id, CancellationToken cancellationToken = default)
        {
            IEnumerable<GuildInfoResponse> results = await service.GetGuildInfosAsync(new[] { id }, cancellationToken).ConfigureAwait(false);
            return results.FirstOrDefault();
        }
    }
}
