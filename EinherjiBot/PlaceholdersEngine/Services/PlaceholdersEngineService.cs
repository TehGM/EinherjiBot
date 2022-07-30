using TehGM.EinherjiBot.PlaceholdersEngine.Placeholders;
using System.Text;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Services
{
    /// <inheritdoc/>
    internal class PlaceholdersEngineService : IPlaceholdersEngine
    {
        private readonly IPlaceholdersProvider _provider;
        private readonly IServiceProvider _services;
        private readonly ILogger _log;

        public PlaceholdersEngineService(IPlaceholdersProvider provider, IServiceProvider services, ILogger<PlaceholdersEngineService> log)
        {
            this._provider = provider;
            this._services = services;
            this._log = log;
        }

        /// <inheritdoc/>
        public async Task<string> ConvertPlaceholdersAsync(string text, CancellationToken cancellationToken = default)
            => await this.ConvertPlaceholdersAsync(text, this._services, cancellationToken);

        /// <inheritdoc/>
        public async Task<string> ConvertPlaceholdersAsync(string text, IServiceProvider services, CancellationToken cancellationToken = default)
        {
            this._log.LogDebug("Running placeholders engine for text {Text}", text);

            StringBuilder builder = new StringBuilder(text);
            foreach (KeyValuePair<PlaceholderAttribute, Type> placeholderInfo in this._provider.GetRegisteredPlaceholders())
            {
                IEnumerable<Match> matches = placeholderInfo.Key.PlaceholderRegex
                    .Matches(builder.ToString())
                    .Where(m => m != null && m.Success);

                if (!matches.Any())
                    continue;

                IPlaceholder placeholder = this._provider.CreateInstance(services, placeholderInfo.Value);
                foreach (Match match in matches.OrderByDescending(m => m.Index))
                {
                    string replacement = await placeholder.GetReplacementAsync(match, cancellationToken).ConfigureAwait(false);
                    builder.Remove(match.Index, match.Length);
                    builder.Insert(match.Index, replacement);
                }
            }
            return builder.ToString();
        }
    }
}
