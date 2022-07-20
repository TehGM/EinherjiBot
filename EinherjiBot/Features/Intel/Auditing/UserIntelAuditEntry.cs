using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Auditing.Intel
{
    public class UserIntelAuditEntry : BotAuditEntry
    {
        [BsonElement("requestedUserID")]
        public ulong RequestedUserID { get; }

        [BsonConstructor(nameof(UserID), nameof(RequestedUserID), nameof(Timestamp), nameof(ExpirationTimestamp))]
        private UserIntelAuditEntry(ulong? userID, ulong requestedUserID, DateTime timestamp, TimeSpan? expiration) 
            : base(userID, timestamp, expiration)
        {
            this.RequestedUserID = requestedUserID;
        }

        public UserIntelAuditEntry(ulong userID, ulong requestedUserID, DateTime timestamp)
            : this(userID, requestedUserID, timestamp, DefaultExpiration) { }
    }
}
