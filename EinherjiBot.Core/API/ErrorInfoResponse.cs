using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TehGM.EinherjiBot.API
{
    public class ErrorInfoResponse : IErrorInfo
    {
        [JsonProperty("timestamp", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(UnixTimestampConverter))]
        public UnixTimestamp Timestamp { get; init; }
        [JsonProperty("message")]
        public string Message { get; init; }

        DateTime IErrorInfo.Timestamp => this.Timestamp.ToDateTime();

        [JsonConstructor]
        private ErrorInfoResponse() { }

        public ErrorInfoResponse(UnixTimestamp timestamp, string message)
        {
            this.Timestamp = timestamp;
            this.Message = message;
        }
    }
}
