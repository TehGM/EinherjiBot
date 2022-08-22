using MongoDB.Bson.Serialization.Attributes;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot
{
    public class ErrorInfo
    {
        [BsonElement("timestamp")]
        public DateTime? Timestamp { get; }
        [BsonElement("message")]
        public string Message { get; }

        [BsonConstructor(nameof(Timestamp), nameof(Message))]
        public ErrorInfo(DateTime? timestamp, string message)
        {
            this.Timestamp = timestamp;
            this.Message = message;
        }

        public static explicit operator ErrorInfoResponse(ErrorInfo error)
        {
            if (error is null)
                return null;
            return new ErrorInfoResponse(
                error.Timestamp != null ? new UnixTimestamp(error.Timestamp.Value) : null,
                error.Message);
        }
    }
}
