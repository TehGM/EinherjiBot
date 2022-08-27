using System.Net;

namespace TehGM.EinherjiBot.API
{
    public class BadRequestException : ApiException
    {
        public IEnumerable<string> Errors { get; }

        public BadRequestException(string message, IEnumerable<string> errors, Exception innerException)
            : base(message ?? errors?.FirstOrDefault(), innerException, HttpStatusCode.BadRequest)
        {
            this.Errors = errors ?? Enumerable.Empty<string>();
        }
        public BadRequestException(string message, IEnumerable<string> errors)
            : this(message, errors, null) { }
        public BadRequestException(IEnumerable<string> errors, Exception innerException)
            : this(null, errors, innerException) { }
        public BadRequestException(IEnumerable<string> errors)
            : this(null, errors, null) { }


        public BadRequestException(string message, Exception innerException)
            : this(message, null, innerException) { }
        public BadRequestException(string message)
            : this(message, null, null) { }
        public BadRequestException()
            : this(null, null, null) { }
    }
}
