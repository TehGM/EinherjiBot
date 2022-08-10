namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    public interface IPlaceholdersProvider
    {
        /// <summary>Adds placeholder to the engine.</summary>
        /// <param name="placeholderType">Type of the placeholder.</param>
        /// <param name="handlerType">Type of the handler.</param>
        /// <exception cref="InvalidOperationException">Placeholder type isn't valid. See message for more details.</exception>
        /// <returns>Whether a new placeholder was successfully added. Will return false if it was already added previously.</returns>
        bool AddPlaceholder(Type placeholderType, Type handlerType, bool validateHandler = true);
        /// <summary>Gets information for all registered placeholders.</summary>
        /// <returns>Descriptors for all registered placeholders.</returns>
        IEnumerable<PlaceholderDescriptor> GetRegisteredPlaceholders();
    }
}
