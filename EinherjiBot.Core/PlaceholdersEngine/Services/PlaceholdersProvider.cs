using System.Reflection;
using System.Runtime.CompilerServices;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Services
{
    public class PlaceholdersProvider : IPlaceholdersProvider
    {
        private readonly IDictionary<string, PlaceholderDescriptor> _placeholders;
        private readonly ILogger _log;

        public PlaceholdersProvider(ILogger<PlaceholdersProvider> log)
        {
            this._placeholders = new Dictionary<string, PlaceholderDescriptor>(StringComparer.OrdinalIgnoreCase);
            this._log = log;
        }

        /// <inheritdoc/>
        public bool AddPlaceholder(Type placeholderType, Type handlerType, bool validateHandler = true)
        {
            this._log.LogTrace("Adding status placeholder type {Type}", placeholderType);

            if (validateHandler)
            {
                if (handlerType == null)
                    throw new InvalidOperationException($"Cannot add placeholder type {placeholderType.FullName} because it has no matching handler.");
                if (handlerType.IsAbstract)
                    throw new InvalidOperationException($"Cannot add placeholder type {placeholderType.FullName} because associated handler is abstract.");
            }

            if (!placeholderType.IsClass)
                throw new InvalidOperationException($"Cannot add placeholder type {placeholderType.FullName} because it's not a class.");
            if (placeholderType.IsAbstract)
                throw new InvalidOperationException($"Cannot add placeholder type {placeholderType.FullName} because it's abstract.");
            if (placeholderType.IsGenericType)
                throw new InvalidOperationException($"Cannot add placeholder type {placeholderType.FullName} because it's generic.");
            if (Attribute.IsDefined(placeholderType, typeof(CompilerGeneratedAttribute)))
                throw new InvalidOperationException($"Cannot add placeholder type {placeholderType.FullName} because it's compiler-generated.");

            PlaceholderAttribute placeholder = placeholderType.GetCustomAttribute<PlaceholderAttribute>();
            if (placeholder == null)
                throw new InvalidOperationException($"Cannot add placeholder type {placeholderType.FullName} because it isn't decorated with {nameof(PlaceholderAttribute)}.");

            PlaceholderDescriptor descriptor = new PlaceholderDescriptor(placeholderType, handlerType);
            if (this._placeholders.TryAdd(placeholder.Identifier, descriptor))
            {
                this._log.LogDebug("Added placeholder {Placeholder}", placeholder.Identifier);
                return true;
            }
            else
            {
                this._log.LogWarning("Cannot add placeholder {Placeholder} as it was already added before", placeholder.Identifier);
                return false;
            }
        }

        /// <inheritdoc/>
        public IEnumerable<PlaceholderDescriptor> GetRegisteredPlaceholders()
            => this._placeholders.Values;
    }
}
