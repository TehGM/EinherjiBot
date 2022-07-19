using Discord;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Intel
{
    public class UserIntelStatusEntry : IUserIntelHistoryEntry
    {
        [BsonElement("isOnline")]
        public bool IsOnline { get; }
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; }

        [BsonConstructor(nameof(IsOnline), nameof(Timestamp))]
        public UserIntelStatusEntry(bool online, DateTime timestamp)
        {
            this.IsOnline = online;
            this.Timestamp = timestamp;
        }

        public bool MatchesStatus(UserStatus status)
            => this.IsOnline == status.IsOnlineStatus();
    }
}
