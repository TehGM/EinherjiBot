namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    public interface IPlaceholderSerializer
    {
        string Serialize(object placeholder);
        object Deserialize(string placeholderValue, Type placeholderType);
    }
}
