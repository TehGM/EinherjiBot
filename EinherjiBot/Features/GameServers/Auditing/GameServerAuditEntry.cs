using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Auditing.GameServer
{
    public class GameServerAuditEntry : BotAuditEntry
    {
        [BsonElement("serverID")]
        public Guid ServerID { get; }

        [BsonConstructor(nameof(UserID), nameof(ServerID), nameof(Timestamp), nameof(ExpirationTimestamp))]
        private GameServerAuditEntry(ulong? userID, Guid serverID, DateTime timestamp, DateTime? expirationTimestamp) 
            : base(userID, RetrieveAction, timestamp, expirationTimestamp)
        {
            this.ServerID = serverID;
        }

        public GameServerAuditEntry(ulong userID, Guid serverID, DateTime timestamp)
            : this(userID, serverID, timestamp, timestamp + DefaultExpiration) { }
    }
}
