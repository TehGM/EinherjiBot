using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Auditing
{
    public abstract class BotAuditEntry
    {
        public const string CreateAction = "Create";
        public const string RetrieveAction = "Retrieve";
        public const string UpdateAction = "Update";
        public const string DeleteAction = "Delete";

        public static TimeSpan DefaultExpiration => TimeSpan.FromDays(14);

        [BsonId]
        public Guid ID { get; set; }
        [BsonElement("action")]
        public string Action { get; }
        [BsonElement("userID")]
        public ulong? UserID { get; }
        [BsonElement("timestamp"), BsonRequired]
        public DateTime Timestamp { get; }
        [BsonElement("expirationTimestamp")]
        public DateTime? ExpirationTimestamp { get; }

        public BotAuditEntry(ulong? userID, string action, DateTime timestamp, DateTime? expirationTimestamp)
        {
            this.ID = Guid.NewGuid();
            this.Action = action;
            this.UserID = userID;
            this.Timestamp = timestamp;
            this.ExpirationTimestamp = expirationTimestamp;
        }
    }
}
