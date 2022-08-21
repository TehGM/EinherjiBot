using TehGM.EinherjiBot.PlaceholdersEngine;

namespace TehGM.EinherjiBot.UI.PlaceholdersEngine.Components
{
    public record PropertyChangedEventArgs(PlaceholderPropertyDescriptor Property, object Value, bool IsValid);
}
