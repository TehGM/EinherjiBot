using Discord;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Intel
{
    public class UserOnlineHistoryEntry
    {
        [BsonElement("onlineStatus")]
        public bool IsOnline { get; }
        [BsonElement("onlineStatusChangeTimestamp")]
        public DateTime Timestamp { get; }

        [BsonConstructor(nameof(IsOnline), nameof(Timestamp))]
        public UserOnlineHistoryEntry(bool online, DateTime timestamp)
        {
            this.IsOnline = online;
            this.Timestamp = timestamp;
        }

        public bool MatchesStatus(UserStatus status)
            => this.IsOnline == status.IsOnlineStatus();
    }
}
