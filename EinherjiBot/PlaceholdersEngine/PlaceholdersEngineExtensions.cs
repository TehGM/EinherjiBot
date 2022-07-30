using TehGM.EinherjiBot.PlaceholdersEngine.Placeholders;

namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    /// <summary>Extensions for <see cref="IPlaceholdersEngine"/>.</summary>
    public static class PlaceholdersEngineExtensions
    {
        /// <summary>Adds placeholder to the engine.</summary>
        /// <param name="engine">Placeholder engine to add placeholder to.</param>
        /// <typeparam name="T">Type of the placeholder.</typeparam>
        /// <exception cref="InvalidOperationException">Placeholder type isn't valid. See message for more details.</exception>
        /// <returns>Whether a new placeholder was successfully added. Will return false if it was already added previously.</returns>
        public static bool AddPlaceholder<T>(this IPlaceholdersEngine engine) where T : IPlaceholder
            => engine.AddPlaceholder(typeof(T));

        /// <summary>Adds placeholders to the engine.</summary>
        /// <param name="engine">Placeholder engine to add placeholders to.</param>
        /// <param name="types">Types of the placeholders.</param>
        /// <exception cref="InvalidOperationException">Any placeholder type isn't valid. See message for more details.</exception>
        /// <returns>Count of added placeholders. Will be less than count of <see cref="types"/> if some placeholders were already added previously.</returns>
        public static int AddPlaceholders(this IPlaceholdersEngine engine, IEnumerable<Type> types)
            => types.Count(t => engine.AddPlaceholder(t));
    }
}
