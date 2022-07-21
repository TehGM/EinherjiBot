namespace TehGM.EinherjiBot.RandomStatus
{
    /// <summary>Regex-based engine for replacing placeholders in text using strictly typed placeholder classes.</summary>
    public interface IStatusPlaceholderEngine
    {
        /// <summary>Adds placeholder to the engine.</summary>
        /// <param name="type">Type of the placeholder.</param>
        /// <exception cref="InvalidOperationException">Placeholder type isn't valid. See message for more details.</exception>
        /// <returns>Whether a new placeholder was successfully added. Will return false if it was already added previously.</returns>
        bool AddPlaceholder(Type type);
        /// <summary>Replaces all known placeholders in a status with actual values.</summary>
        /// <remarks>Each placeholder instance will only be created if needed, and will always be scoped to one call of this method.</remarks>
        /// <param name="status">Status to replace placeholders in.</param>
        /// <param name="cancellationToken">Token to cancel any asynchronous operation.</param>
        /// <returns>Status with all placeholders replaced.</returns>
        Task<string> ConvertPlaceholdersAsync(string status, CancellationToken cancellationToken = default);
    }
}
