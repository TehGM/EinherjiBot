using System.Net;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    [Serializable]
    public class PlaceholderContextException : ApiException
    {
        public PlaceholderContextException(PlaceholderDescriptor placeholder, Exception innerException = null)
            : this($"Placeholder {placeholder.Identifier} is not usable in this context.", innerException) { }

        public PlaceholderContextException(string message, Exception innerException = null)
            : base(message, innerException, HttpStatusCode.BadRequest) { }
    }
}
