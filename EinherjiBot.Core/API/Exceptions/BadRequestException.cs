using System.Net;

namespace TehGM.EinherjiBot.API
{
    public class BadRequestException : ApiException
    {
        public BadRequestException(string message, Exception innerException)
            : base(message, innerException, HttpStatusCode.BadRequest) { }
        public BadRequestException(string message)
            : this(message, null) { }
        public BadRequestException()
            : this(null) { }
    }
}
