using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Runtime.CompilerServices;
using TehGM.EinherjiBot.PlaceholdersEngine.Placeholders;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Services
{
    internal class PlaceholdersProvider : IPlaceholdersProvider
    {
        private readonly IDictionary<PlaceholderAttribute, Type> _placeholders;
        private readonly ILogger _log;

        public PlaceholdersProvider(ILogger<PlaceholdersEngineService> log)
        {
            this._placeholders = new Dictionary<PlaceholderAttribute, Type>();
            this._log = log;
        }

        /// <inheritdoc/>
        public bool AddPlaceholder(Type type)
        {
            this._log.LogTrace("Adding status placeholder type {Type}", type);

            if (!type.IsClass)
                throw new InvalidOperationException($"Cannot add placeholder type {type.FullName} because it's not a class.");
            if (type.IsAbstract)
                throw new InvalidOperationException($"Cannot add placeholder type {type.FullName} because it's abstract.");
            if (type.IsGenericType)
                throw new InvalidOperationException($"Cannot add placeholder type {type.FullName} because it's generic.");
            if (!typeof(IPlaceholder).IsAssignableFrom(type))
                throw new InvalidOperationException($"Cannot add placeholder type {type.FullName} because it doesn't implement {nameof(IPlaceholder)} interface.");
            if (Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute)))
                throw new InvalidOperationException($"Cannot add placeholder type {type.FullName} because it's compiler-generated.");

            PlaceholderAttribute placeholder = type.GetCustomAttribute<PlaceholderAttribute>();
            if (placeholder == null)
                throw new InvalidOperationException($"Cannot add placeholder type {type.FullName} because it isn't decorated with {nameof(PlaceholderAttribute)}.");

            if (this._placeholders.TryAdd(placeholder, type))
            {
                this._log.LogDebug("Added placeholder {Placeholder}", placeholder.GetDisplayText());
                return true;
            }
            else
            {
                this._log.LogWarning("Cannot add placeholder {Placeholder} as it was already added before", placeholder.GetDisplayText());
                return false;
            }
        }

        /// <inheritdoc/>
        public IPlaceholder CreateInstance(IServiceProvider services, Type type)
            => (IPlaceholder)ActivatorUtilities.CreateInstance(services, type);

        /// <inheritdoc/>
        public IEnumerable<KeyValuePair<PlaceholderAttribute, Type>> GetRegisteredPlaceholders()
            => this._placeholders.AsEnumerable();
    }
}
