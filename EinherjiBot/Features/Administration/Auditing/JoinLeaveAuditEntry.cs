﻿using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Auditing.Administration
{
    public class JoinLeaveAuditEntry : BotAuditEntry
    {
        public const string JoinAction = "Joined";
        public const string LeaveAction = "Left";

        [BsonElement("guildID")]
        public ulong GuildID { get; }

        [BsonConstructor(nameof(UserID), nameof(GuildID), nameof(Action), nameof(Timestamp), nameof(ExpirationTimestamp))]
        private JoinLeaveAuditEntry(ulong? userID, ulong guildID, string action, DateTime timestamp, DateTime? expirationTimestamp) 
            : base(userID, action, timestamp, expirationTimestamp)
        {
            this.GuildID = guildID;
        }

        public static JoinLeaveAuditEntry UserJoined(ulong userID, ulong guildID, DateTime timestamp)
            => new JoinLeaveAuditEntry(userID, guildID, JoinAction, timestamp, timestamp + TimeSpan.FromDays(100));
        public static JoinLeaveAuditEntry UserLeft(ulong userID, ulong guildID, DateTime timestamp)
            => new JoinLeaveAuditEntry(userID, guildID, LeaveAction, timestamp, timestamp + TimeSpan.FromDays(100));
    }
}