namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    [Serializable]
    public class PlaceholderContextException : Exception
    {
        public PlaceholderContextException(PlaceholderDescriptor placeholder, Exception innerException = null)
            : base($"Placeholder {placeholder.Identifier} is not usable in this context.", innerException) { }
    }
}
