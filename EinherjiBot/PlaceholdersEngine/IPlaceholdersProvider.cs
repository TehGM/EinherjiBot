using TehGM.EinherjiBot.PlaceholdersEngine.Placeholders;

namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    public interface IPlaceholdersProvider
    {
        /// <summary>Adds placeholder to the engine.</summary>
        /// <param name="type">Type of the placeholder.</param>
        /// <exception cref="InvalidOperationException">Placeholder type isn't valid. See message for more details.</exception>
        /// <returns>Whether a new placeholder was successfully added. Will return false if it was already added previously.</returns>
        bool AddPlaceholder(Type type);
        /// <summary>Creates a new placeholder instance.</summary>
        /// <param name="services">Services to use when resolving placeholder's dependencies.</param>
        /// <param name="type">Type of the placeholder implementation.</param>
        /// <returns>New placeholder instance.</returns>
        IPlaceholder CreateInstance(IServiceProvider services, Type type);
        /// <summary>Gets information for all registered placeholders.</summary>
        /// <returns>Key-value pairs for all registered placeholders.</returns>
        IEnumerable<KeyValuePair<OldPlaceholderAttribute, Type>> GetRegisteredPlaceholders();
    }
}
