namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    [Serializable]
    public class PlaceholderFormatException : Exception
    {
        public Type PlaceholderType { get; }

        public PlaceholderFormatException(string message, Type placeholderType, Exception innerException = null)
            : base(message, innerException)
        {
            this.PlaceholderType = placeholderType;
        }

        public PlaceholderFormatException(string message, Exception innerException = null)
            : this(message, null, innerException) { }
    }
}
