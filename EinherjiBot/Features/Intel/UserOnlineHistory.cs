using Discord;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Intel
{
    public class UserOnlineHistory : ICacheableEntity<ulong>
    {
        public const int MaxHistorySize = 10;

        [BsonId]
        public ulong ID { get; }
        [BsonElement("history")]
        public ICollection<UserOnlineHistoryEntry> History { get; private set; }

        [BsonConstructor(nameof(ID), nameof(History))]
        private UserOnlineHistory(ulong id, ICollection<UserOnlineHistoryEntry> history)
        {
            this.ID = id;
            this.History = history;

            this.TrimHistory();
        }

        public UserOnlineHistory(ulong id)
            : this(id, new List<UserOnlineHistoryEntry>(MaxHistorySize)) { }

        public UserOnlineHistoryEntry GetLatestStatus()
            => this.History.OrderByDescending(status => status.Timestamp).FirstOrDefault();

        private void TrimHistory()
        {
            if (this.History.Count <= MaxHistorySize)
                return;
            this.History = this.History
                .OrderByDescending(status => status.Timestamp)
                .Take(MaxHistorySize)
                .ToList();
        }

        public bool ChangeStatus(bool isOnline)
        {
            UserOnlineHistoryEntry latest = this.GetLatestStatus();
            if (latest != null && latest.IsOnline == isOnline)
                return false;
            this.History.Add(new UserOnlineHistoryEntry(isOnline, DateTime.UtcNow));
            this.TrimHistory();
            return true;
        }

        public bool ChangeStatus(UserStatus status)
            =>this.ChangeStatus(status.IsOnlineStatus());

        public ulong GetCacheKey()
            => this.ID;
    }
}
