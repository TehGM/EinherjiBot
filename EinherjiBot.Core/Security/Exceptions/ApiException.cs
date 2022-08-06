using System.Net;
using System.Net.Http;

namespace TehGM.EinherjiBot.Security
{
    public class ApiException : HttpRequestException
    {
        public ApiException(string message, Exception innerException, HttpStatusCode statusCode)
            : base(message, innerException, statusCode) { }
        public ApiException(string message, HttpStatusCode statusCode)
            : this(message, null, statusCode) { }
    }
}
