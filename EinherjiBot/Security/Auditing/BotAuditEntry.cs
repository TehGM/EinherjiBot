using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Auditing
{
    public abstract class BotAuditEntry
    {
        public static TimeSpan DefaultExpiration => TimeSpan.FromDays(14);

        [BsonId]
        public Guid ID { get; set; }
        [BsonElement("userID")]
        public ulong? UserID { get; }
        [BsonElement("timestamp"), BsonRequired]
        public DateTime Timestamp { get; }
        [BsonElement("expirationTimestamp")]
        public DateTime? ExpirationTimestamp { get; }

        public BotAuditEntry(ulong? userID, DateTime timestamp, TimeSpan? expiration)
        {
            this.ID = Guid.NewGuid();
            this.UserID = userID;
            this.Timestamp = timestamp;
            this.ExpirationTimestamp = timestamp + expiration;
        }
    }
}
