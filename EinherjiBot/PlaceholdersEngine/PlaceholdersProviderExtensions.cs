using System.Reflection;
using System.Runtime.CompilerServices;

namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    /// <summary>Extensions for <see cref="IPlaceholdersEngine"/>.</summary>
    public static class PlaceholdersProviderExtensions
    {
        /// <summary>Adds placeholder to the provider.</summary>
        /// <param name="provider">Placeholder provider to add placeholder to.</param>
        /// <typeparam name="TPlaceholder">Type of the placeholder.</typeparam>
        /// <typeparam name="THandler">Type of the handler</typeparam>
        /// <exception cref="InvalidOperationException">Placeholder type isn't valid. See message for more details.</exception>
        /// <returns>Whether a new placeholder was successfully added. Will return false if it was already added previously.</returns>
        public static bool AddPlaceholder<TPlaceholder, THandler>(this IPlaceholdersProvider provider) where TPlaceholder : class where THandler : IPlaceholderHandler
            => provider.AddPlaceholder(typeof(TPlaceholder), typeof(THandler));

        /// <summary>Adds placeholders to the provider.</summary>
        /// <param name="provider">Placeholder provider to add placeholders to.</param>
        /// <param name="types">Types of the placeholders and their matched handlers.</param>
        /// <exception cref="InvalidOperationException">Any placeholder type isn't valid. See message for more details.</exception>
        /// <returns>Count of added placeholders. Will be less than count of <see cref="types"/> if some placeholders were already added previously.</returns>
        public static int AddPlaceholders(this IPlaceholdersProvider provider, IEnumerable<KeyValuePair<Type, Type>> types)
            => types.Count(t => provider.AddPlaceholder(t.Key, t.Value));

        public static int AddPlaceholders(this IPlaceholdersProvider provider, params Assembly[] assemblies)
        {
            IEnumerable<Type> foundTypes = assemblies.SelectMany(a => a.DefinedTypes.Where(t => !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute))));
            IEnumerable<Type> placeholderTypes = foundTypes.Where(t => Attribute.IsDefined(t, typeof(PlaceholderAttribute), true));
            IEnumerable<Type> handlerTypes = foundTypes.Where(t => typeof(IPlaceholderHandler).IsAssignableFrom(t));

            // we purposefully allow null/anstract handler types to let provider's implementation throw the exception
            IDictionary<Type, Type> placeholders = placeholderTypes.ToDictionary(t => t,
                t => handlerTypes.FirstOrDefault(t => t.IsGenericType && !t.ContainsGenericParameters && t.GenericTypeArguments.First() == t);

            return provider.AddPlaceholders(placeholders);
        }
    }
}
