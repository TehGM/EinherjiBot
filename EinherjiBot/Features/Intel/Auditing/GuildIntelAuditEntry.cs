using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Auditing.Intel
{
    public class GuildIntelAuditEntry : BotAuditEntry
    {
        [BsonElement("requestedGuildID")]
        public ulong RequestedGuildID { get; }

        [BsonConstructor(nameof(UserID), nameof(RequestedGuildID), nameof(Timestamp), nameof(ExpirationTimestamp))]
        private GuildIntelAuditEntry(ulong? userID, ulong requestedGuildID, DateTime timestamp, TimeSpan? expiration) 
            : base(userID, timestamp, expiration)
        {
            this.RequestedGuildID = requestedGuildID;
        }

        public GuildIntelAuditEntry(ulong userID, ulong requestedUserID, DateTime timestamp)
            : this(userID, requestedUserID, timestamp, DefaultExpiration) { }
    }
}
