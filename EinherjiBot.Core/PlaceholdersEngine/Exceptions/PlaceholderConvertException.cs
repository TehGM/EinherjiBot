using System.Net;

namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    [Serializable]
    public class PlaceholderConvertException : PlaceholderException
    {
        public PlaceholderConvertException(string message, Type placeholderType, Exception innerException = null)
            : base(message, innerException, HttpStatusCode.UnprocessableEntity)
        {
            this.PlaceholderType = placeholderType;
        }

        public PlaceholderConvertException(string message, Exception innerException = null)
            : this(message, null, innerException) { }
    }
}
