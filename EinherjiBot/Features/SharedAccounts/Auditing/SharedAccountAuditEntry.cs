using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Auditing.SharedAccounts
{
    public class SharedAccountAuditEntry : BotAuditEntry
    {
        [BsonElement("accountID")]
        public Guid AccountID { get; }

        [BsonConstructor(nameof(UserID), nameof(AccountID), nameof(Action), nameof(Timestamp), nameof(ExpirationTimestamp))]
        private SharedAccountAuditEntry(ulong? userID, Guid accountID, string action, DateTime timestamp, DateTime? expirationTimestamp)
            : base(userID, action, timestamp, expirationTimestamp)
        {
            this.AccountID = accountID;
        }

        public static SharedAccountAuditEntry Retrieved(ulong userID, Guid accountID, DateTime timestamp)
            => new SharedAccountAuditEntry(userID, accountID, RetrieveAction, timestamp, timestamp + TimeSpan.FromDays(30));
        public static SharedAccountAuditEntry Updated(ulong userID, Guid accountID, DateTime timestamp)
            => new SharedAccountAuditEntry(userID, accountID, UpdateAction, timestamp, timestamp + TimeSpan.FromDays(30));
        public static SharedAccountAuditEntry Created(ulong userID, Guid accountID, DateTime timestamp)
            => new SharedAccountAuditEntry(userID, accountID, CreateAction, timestamp, timestamp + TimeSpan.FromDays(30));
        public static SharedAccountAuditEntry Deleted(ulong userID, Guid accountID, DateTime timestamp)
            => new SharedAccountAuditEntry(userID, accountID, DeleteAction, timestamp, timestamp + TimeSpan.FromDays(30));
    }
}
