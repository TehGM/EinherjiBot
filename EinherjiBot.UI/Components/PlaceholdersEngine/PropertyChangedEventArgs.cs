using TehGM.EinherjiBot.PlaceholdersEngine;

namespace TehGM.EinherjiBot.UI.Components.PlaceholdersEngine
{
    public record PropertyChangedEventArgs(PlaceholderPropertyDescriptor Property, object Value, bool IsValid);
}
