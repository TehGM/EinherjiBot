using System.Net;

namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    [Serializable]
    public class PlaceholderFormatException : PlaceholderException
    {
        public PlaceholderFormatException(string message, Type placeholderType, Exception innerException = null)
            : base(message, innerException, HttpStatusCode.BadRequest)
        {
            this.PlaceholderType = placeholderType;
        }

        public PlaceholderFormatException(string message, Exception innerException = null)
            : this(message, null, innerException) { }
    }
}
