using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Auditing.Settings
{
    public class GuildSettingsAuditEntry : BotAuditEntry
    {
        [BsonElement("guildID")]
        public ulong GuildID { get; }

        [BsonConstructor(nameof(UserID), nameof(GuildID), nameof(Action), nameof(Timestamp), nameof(ExpirationTimestamp))]
        private GuildSettingsAuditEntry(ulong? userID, ulong guildID, string action, DateTime timestamp, DateTime? expirationTimestamp)
            : base(userID, action, timestamp, expirationTimestamp)
        {
            this.GuildID = guildID;
        }

        public static GuildSettingsAuditEntry Updated(ulong userID, ulong guildID, DateTime timestamp)
            => new GuildSettingsAuditEntry(userID, guildID, UpdateAction, timestamp, timestamp + TimeSpan.FromDays(30));
    }
}
