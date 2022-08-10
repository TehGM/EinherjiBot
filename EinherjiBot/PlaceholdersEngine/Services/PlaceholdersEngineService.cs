using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Services
{
    /// <inheritdoc/>
    internal class PlaceholdersEngineService : IPlaceholdersEngine
    {
        private readonly IPlaceholdersProvider _provider;
        private readonly IPlaceholderSerializer _serializer;
        private readonly IServiceProvider _services;
        private readonly IAuthContext _auth;
        private readonly ILogger _log;

        public PlaceholdersEngineService(IPlaceholdersProvider provider, IPlaceholderSerializer serializer,
            IAuthContext auth, IServiceProvider services, ILogger<PlaceholdersEngineService> log)
        {
            this._provider = provider;
            this._serializer = serializer;
            this._auth = auth;
            this._services = services;
            this._log = log;
        }

        /// <inheritdoc/>
        public async Task<string> ConvertPlaceholdersAsync(string text, PlaceholderUsage context, CancellationToken cancellationToken = default)
            => await this.ConvertPlaceholdersAsync(text, context, this._services, cancellationToken);

        /// <inheritdoc/>
        public async Task<string> ConvertPlaceholdersAsync(string text, PlaceholderUsage context, IServiceProvider services, CancellationToken cancellationToken = default)
        {
            this._log.LogDebug("Running placeholders engine for text {Text}", text);

            if (this._auth.IsAdmin() || this._auth.IsEinherji())
                context |= PlaceholderUsage.Admin;

            StringBuilder builder = new StringBuilder(text);
            foreach (PlaceholderDescriptor descriptor in this._provider.GetRegisteredPlaceholders())
            {
                IEnumerable<Match> matches = descriptor.MatchingRegex
                    .Matches(builder.ToString())
                    .Where(m => m != null && m.Success);

                if (!matches.Any())
                    continue;

                if (!descriptor.AvailableInContext(context))
                    throw new PlaceholderContextException(descriptor);

                IPlaceholderHandler handler = (IPlaceholderHandler)ActivatorUtilities.CreateInstance(services, descriptor.HandlerType);
                foreach (Match match in matches.OrderByDescending(m => m.Index))
                {
                    object placehoder = this._serializer.Deserialize(match.Value, descriptor.PlaceholderType);
                    string replacement = await handler.GetReplacementAsync(placehoder, cancellationToken).ConfigureAwait(false);
                    builder.Remove(match.Index, match.Length);
                    builder.Insert(match.Index, replacement);
                }
            }
            return builder.ToString();
        }
    }
}
