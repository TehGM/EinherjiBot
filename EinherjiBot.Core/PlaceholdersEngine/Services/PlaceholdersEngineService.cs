using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Services
{
    /// <inheritdoc/>
    public class PlaceholdersEngineService : IPlaceholdersEngine
    {
        private readonly IPlaceholdersProvider _provider;
        private readonly IPlaceholderSerializer _serializer;
        private readonly IServiceProvider _services;
        private readonly IAuthProvider _auth;
        private readonly ILogger _log;

        public PlaceholdersEngineService(IPlaceholdersProvider provider, IPlaceholderSerializer serializer,
            IAuthProvider auth, IServiceProvider services, ILogger<PlaceholdersEngineService> log)
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

            IBotAuthorizationService authService = services.GetRequiredService<IBotAuthorizationService>();
            if (this._auth.User.IsAdmin() || this._auth.User.IsEinherji())
                context |= PlaceholderUsage.Admin;

            ICollection<PlaceholderMatch> foundMatches = new List<PlaceholderMatch>();
            foreach (PlaceholderDescriptor descriptor in this._provider.GetRegisteredPlaceholders())
            {
                IEnumerable<Match> matches = descriptor.MatchingRegex
                    .Matches(text)
                    .Where(m => m?.Success == true);

                if (!matches.Any())
                    continue;

                if (!descriptor.AvailableInContext(context))
                    throw new PlaceholderContextException(descriptor);

                if (descriptor.Policies.Any())
                {
                    BotAuthorizationResult authorization = await authService.AuthorizeAsync(descriptor.Policies, cancellationToken).ConfigureAwait(false);
                    if (!authorization.Succeeded)
                        throw new AccessForbiddenException($"You have no permissions to use this placeholder.");
                }

                if (descriptor.HandlerType == null)
                    throw new InvalidOperationException("Can't convert placeholder with no handler assigned.");

                IPlaceholderHandler handler = (IPlaceholderHandler)ActivatorUtilities.CreateInstance(services, descriptor.HandlerType);
                foreach (Match match in matches)
                    foundMatches.Add(new PlaceholderMatch(match, descriptor, handler));
            }

            StringBuilder builder = new StringBuilder(text);
            foreach (PlaceholderMatch match in foundMatches.OrderByDescending(m => m.RegexMatch.Index))
            {
                object placehoder = this._serializer.Deserialize(match.RegexMatch.Value, match.Descriptor.PlaceholderType);
                string replacement = await match.Handler.GetReplacementAsync(placehoder, cancellationToken).ConfigureAwait(false);
                builder.Remove(match.RegexMatch.Index, match.RegexMatch.Length);
                builder.Insert(match.RegexMatch.Index, replacement);
            }
            return builder.ToString();
        }

        private record PlaceholderMatch(Match RegexMatch, PlaceholderDescriptor Descriptor, IPlaceholderHandler Handler);
    }
}
