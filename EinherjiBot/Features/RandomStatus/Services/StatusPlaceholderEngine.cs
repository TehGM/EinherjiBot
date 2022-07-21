using TehGM.EinherjiBot.RandomStatus.Placeholders;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace TehGM.EinherjiBot.RandomStatus.Services
{
    /// <inheritdoc/>
    internal class StatusPlaceholderEngine : IStatusPlaceholderEngine
    {
        private readonly IDictionary<StatusPlaceholderAttribute, Type> _placeholders;
        private readonly IServiceScopeFactory _services;
        private readonly ILogger _log;

        public StatusPlaceholderEngine(IServiceScopeFactory services, ILogger<StatusPlaceholderEngine> log)
        {
            this._placeholders = new Dictionary<StatusPlaceholderAttribute, Type>();
            this._services = services;
            this._log = log;
        }

        /// <inheritdoc/>
        public bool AddPlaceholder(Type type)
        {
            this._log.LogTrace("Adding status placeholder type {Type}", type);

            if (!type.IsClass)
                throw new InvalidOperationException($"Cannot add status placeholder type {type.FullName} because it's not a class.");
            if (type.IsAbstract)
                throw new InvalidOperationException($"Cannot add status placeholder type {type.FullName} because it's abstract.");
            if (type.IsGenericType)
                throw new InvalidOperationException($"Cannot add status placeholder type {type.FullName} because it's generic.");
            if (!typeof(IStatusPlaceholder).IsAssignableFrom(type))
                throw new InvalidOperationException($"Cannot add status placeholder type {type.FullName} because it doesn't implement {nameof(IStatusPlaceholder)} interface.");
            if (Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute)))
                throw new InvalidOperationException($"Cannot add status placeholder type {type.FullName} because it's compiler-generated.");

            StatusPlaceholderAttribute placeholder = type.GetCustomAttribute<StatusPlaceholderAttribute>();
            if (placeholder == null)
                throw new InvalidOperationException($"Cannot add status placeholder type {type.FullName} because it isn't decorated with {nameof(StatusPlaceholderAttribute)}.");

            if (this._placeholders.TryAdd(placeholder, type))
            {
                this._log.LogDebug("Added status placeholder {Placeholder}", placeholder.Placeholder);
                return true;
            }
            else
            {
                this._log.LogWarning("Cannot add status placeholder {Placeholder} as it was already added before", placeholder.Placeholder);
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<string> ConvertPlaceholdersAsync(string status, CancellationToken cancellationToken = default)
        {
            this._log.LogDebug("Running placeholders engine for status {Status}", status);

            using IServiceScope services = this._services.CreateScope();
            StringBuilder builder = new StringBuilder(status);
            foreach (KeyValuePair<StatusPlaceholderAttribute, Type> placeholderInfo in this._placeholders)
            {
                IEnumerable<Match> matches = placeholderInfo.Key.PlaceholderRegex
                    .Matches(builder.ToString())
                    .Where(m => m != null && m.Success);

                if (!matches.Any())
                    continue;

                IStatusPlaceholder placeholder = (IStatusPlaceholder)ActivatorUtilities.CreateInstance(services.ServiceProvider, placeholderInfo.Value);

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
