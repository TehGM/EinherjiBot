using System.Net;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    [Serializable]
    public class PlaceholderException : ApiException
    {
        public Type PlaceholderType { get; init; }

        public PlaceholderException(string message, Exception innerException, HttpStatusCode statusCode)
            : base(message, innerException, statusCode) { }
        public PlaceholderException(string message, HttpStatusCode statusCode)
            : this(message, null, statusCode) { }
    }
}
