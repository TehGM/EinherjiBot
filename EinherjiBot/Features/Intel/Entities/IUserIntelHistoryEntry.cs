using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Intel
{
    public interface IUserIntelHistoryEntry
    {
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; }
    }
}
