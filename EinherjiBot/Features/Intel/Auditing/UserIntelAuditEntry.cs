using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Auditing.Intel
{
    public class UserIntelAuditEntry : BotAuditEntry
    {
        [BsonElement("requestedUserID")]
        public ulong RequestedUserID { get; }

        [BsonConstructor(nameof(UserID), nameof(RequestedUserID), nameof(Timestamp), nameof(ExpirationTimestamp))]
        private UserIntelAuditEntry(ulong? userID, ulong requestedUserID, DateTime timestamp, DateTime? expirationTimestamp) 
            : base(userID, RetrieveAction, timestamp, expirationTimestamp)
        {
            this.RequestedUserID = requestedUserID;
        }

        public UserIntelAuditEntry(ulong userID, ulong requestedUserID, DateTime timestamp)
            : this(userID, requestedUserID, timestamp, timestamp + DefaultExpiration) { }
    }
}
