namespace TehGM.EinherjiBot.API
{
    public interface IDiscordEntityInfoService
    {
        // role
        /// <summary>Gets Role entity info for one specific role, with optional guilds restriction.</summary>
        /// <param name="roleID">ID of the role.</param>
        /// <param name="guildIDs">IDs of the guilds to check. If null, all bot's guilds will be checked.</param>
        /// <param name="cancellationToken">Token for operation cancellation.</param>
        /// <returns>Role info if found; otherwise null.</returns>
        Task<RoleInfoResponse> GetRoleInfoAsync(ulong roleID, ulong[] guildIDs, CancellationToken cancellationToken = default);

        // user
        /// <summary>Gets User entity info for bot's user.</summary>
        /// <param name="cancellationToken">Token for operation cancellation.</param>
        /// <returns>Bot user info.</returns>
        ValueTask<UserInfoResponse> GetBotInfoAsync(CancellationToken cancellationToken = default);
        /// <summary>Gets User entity info for one specific user.</summary>
        /// <param name="userID">ID of the user.</param>
        /// <param name="cancellationToken">Token for operation cancellation.</param>
        /// <returns>User info if found; otherwise null.</returns>
        Task<UserInfoResponse> GetUserInfoAsync(ulong userID, CancellationToken cancellationToken = default);

        // guild
        /// <summary>Scans for data on guilds that both user and bot are present in.</summary>
        /// <remarks>Due to privacy concerns, non-admin users will only be able to get info on guilds they're present in.</remarks>
        /// <param name="ids">IDs of the guilds to check. If null, all bot's guilds will be checked.</param>
        /// <param name="cancellationToken">Token for operation cancellation.</param>
        /// <returns>All Guild infos found.</returns>
        /// <exception cref="AccessForbiddenException">User has no access to requested guild.</exception>
        Task<IEnumerable<GuildInfoResponse>> GetGuildInfosAsync(ulong[] ids, CancellationToken cancellationToken = default);

        // guild user
        /// <summary>Gets Guild User entity info for one specific user. Only guilds with bot added will be checked.</summary>
        /// <remarks>Due to privacy concerns, non-admin users will only be able to get info on guilds they're present in.</remarks>
        /// <param name="userID">ID of the user.</param>
        /// <param name="guildID">ID of the guild.</param>
        /// <param name="cancellationToken">Token for operation cancellation.</param>
        /// <returns>Guild User info if found; null if not found, or if bot is not present in requested guild.</returns>
        /// <exception cref="AccessForbiddenException">User has no access to requested guild.</exception>
        Task<GuildUserInfoResponse> GetGuildUserInfoAsync(ulong userID, ulong guildID, CancellationToken cancellationToken = default);
    }
}
