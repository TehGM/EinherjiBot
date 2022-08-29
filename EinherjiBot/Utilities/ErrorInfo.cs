using MongoDB.Bson.Serialization.Attributes;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot
{
    public class ErrorInfo : IErrorInfo
    {
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; }
        [BsonElement("message")]
        public string Message { get; }

        [BsonConstructor(nameof(Timestamp), nameof(Message))]
        public ErrorInfo(DateTime timestamp, string message)
        {
            this.Timestamp = timestamp;
            this.Message = message;
        }

        public static explicit operator ErrorInfoResponse(ErrorInfo error)
        {
            if (error is null)
                return null;
            return new ErrorInfoResponse(
                new UnixTimestamp(error.Timestamp),
                error.Message);
        }

        public static explicit operator ErrorInfo(ErrorInfoResponse error)
        {
            if (error is null)
                return null;
            return new ErrorInfo(error.Timestamp.ToDateTime(), error.Message);
        }
    }
}
