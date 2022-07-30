namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    /// <summary>Regex-based engine for replacing placeholders in text using strictly typed placeholder classes.</summary>
    public interface IPlaceholdersEngine
    {
        /// <summary>Replaces all known placeholders in text with actual values.</summary>
        /// <remarks>Each placeholder instance will only be created if needed, and will always be scoped to one call of this method.</remarks>
        /// <param name="text">Text to replace placeholders in.</param>
        /// <param name="cancellationToken">Token to cancel any asynchronous operation.</param>
        /// <returns>Text with all placeholders replaced.</returns>
        Task<string> ConvertPlaceholdersAsync(string text, CancellationToken cancellationToken = default);
        /// <summary>Replaces all known placeholders in text with actual values.</summary>
        /// <remarks>Each placeholder instance will only be created if needed, and will always be scoped to one call of this method.</remarks>
        /// <param name="text">Text to replace placeholders in.</param>
        /// <param name="services">Services to use when creating placeholders. This allows using scoped services.</param>
        /// <param name="cancellationToken">Token to cancel any asynchronous operation.</param>
        /// <returns>Text with all placeholders replaced.</returns>
        Task<string> ConvertPlaceholdersAsync(string text, IServiceProvider services, CancellationToken cancellationToken = default);
    }
}
