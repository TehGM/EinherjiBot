using System.Net;

namespace TehGM.EinherjiBot.API
{
    public class AccessForbiddenException : ApiException
    {
        public AccessForbiddenException(string message, Exception innerException)
            : base(message, innerException, HttpStatusCode.Forbidden) { }
        public AccessForbiddenException(string message)
            : this(message, null) { }
        public AccessForbiddenException()
            : this("You are not authorized to perform this action") { }
    }
}
