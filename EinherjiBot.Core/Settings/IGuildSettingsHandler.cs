namespace TehGM.EinherjiBot.Settings
{
    public interface IGuildSettingsHandler
    {
        /// <summary>Gets settings for specified guild.</summary>
        /// <remarks>If none exists, a default for guild will be automatically created.</remarks>
        /// <param name="guildID">ID of the guild.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Settings for the guild; null if guild not found.</returns>
        Task<GuildSettingsResponse> GetAsync(ulong guildID, CancellationToken cancellationToken = default);

        /// <summary>Updates settings for specified guild.</summary>
        /// <param name="guildID">ID of the guild.</param>
        /// <param name="request">Data of new settings state.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>Updated settings along with information whether any changes have been made.</returns>
        Task<EntityUpdateResult<GuildSettingsResponse>> UpdateAsync(ulong guildID, GuildSettingsRequest request, CancellationToken cancellationToken = default);
    }
}
