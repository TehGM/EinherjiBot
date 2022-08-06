using System.Net.Http;

namespace TehGM.EinherjiBot.Security
{
    public class AccessForbiddenException : HttpRequestException
    {
        public AccessForbiddenException(string message, Exception innerException)
            : base(message, innerException, System.Net.HttpStatusCode.Forbidden) { }
        public AccessForbiddenException(string message)
            : this(message, null) { }
        public AccessForbiddenException()
            : this("You are not authorized to perform this action") { }
    }
}
