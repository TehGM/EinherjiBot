using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Auditing.SharedAccounts
{
    public class SharedAccountAuditEntry : BotAuditEntry
    {
        [BsonElement("accountID")]
        public Guid AccountID { get; }

        [BsonConstructor(nameof(UserID), nameof(AccountID), nameof(Action), nameof(Timestamp), nameof(ExpirationTimestamp))]
        private SharedAccountAuditEntry(ulong? userID, Guid accountID, string action, DateTime timestamp, TimeSpan? expiration)
            : base(userID, action, timestamp, expiration)
        {
            this.AccountID = accountID;
        }

        public static SharedAccountAuditEntry Retrieved(ulong userID, Guid accountID, DateTime timestamp)
            => new SharedAccountAuditEntry(userID, accountID, RetrieveAction, timestamp, TimeSpan.FromDays(30));
        public static SharedAccountAuditEntry Updated(ulong userID, Guid accountID, DateTime timestamp)
            => new SharedAccountAuditEntry(userID, accountID, UpdateAction, timestamp, TimeSpan.FromDays(30));
    }
}
