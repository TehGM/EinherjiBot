using Discord;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Intel
{
    public class UserIntel : ICacheableEntity<ulong>
    {
        public const int MaxHistorySize = 10;

        [BsonId]
        public ulong ID { get; }
        [BsonElement("statusHistory"), BsonIgnoreIfDefault, BsonDefaultValue(null)]
        public ICollection<UserIntelStatusEntry> StatusHistory { get; private set; }
        [BsonElement("customStatusHistory"), BsonIgnoreIfDefault, BsonDefaultValue(null)]
        public ICollection<UserIntelCustomStatusEntry> CustomStatusHistory { get; private set; }
        [BsonElement("listeningStatusHistory"), BsonIgnoreIfDefault, BsonDefaultValue(null)]
        public ICollection<UserIntelListeningStatusEntry> ListeningStatusHistory { get; private set; }

        [BsonConstructor(nameof(ID), nameof(StatusHistory), nameof(CustomStatusHistory), nameof(ListeningStatusHistory))]
        private UserIntel(ulong id, ICollection<UserIntelStatusEntry> statusHistory,
            ICollection<UserIntelCustomStatusEntry> customStatusHistory, ICollection<UserIntelListeningStatusEntry> listeningStatusHistory)
        {
            this.ID = id;
            this.StatusHistory = statusHistory ?? new List<UserIntelStatusEntry>(MaxHistorySize);
            this.CustomStatusHistory = customStatusHistory ?? new List<UserIntelCustomStatusEntry>(MaxHistorySize);
            this.ListeningStatusHistory = listeningStatusHistory ?? new List<UserIntelListeningStatusEntry>(MaxHistorySize);

            this.TrimStatusHistory();
            this.TrimCustomStatusHistory();
            this.TrimListeningStatusHistory();
        }

        public UserIntel(ulong id)
            : this(id, null, null, null) { }


        public UserIntelStatusEntry GetLatestStatus()
            => this.GetLatestHistoryEntry(this.StatusHistory);
        public UserIntelCustomStatusEntry GetLatestCustomStatus()
            => this.GetLatestHistoryEntry(this.CustomStatusHistory);
        public UserIntelListeningStatusEntry GetLatestListeningStatus()
            => this.GetLatestHistoryEntry(this.ListeningStatusHistory);


        public bool ChangeStatus(bool isOnline)
        {
            UserIntelStatusEntry latest = this.GetLatestStatus();
            if (latest != null && latest.IsOnline == isOnline)
                return false;
            this.StatusHistory.Add(new UserIntelStatusEntry(isOnline, DateTime.UtcNow));
            this.TrimStatusHistory();
            return true;
        }
        public bool ChangeStatus(UserStatus status)
            => this.ChangeStatus(status.IsOnlineStatus());

        public bool ChangeCustomStatus(string text)
        {
            UserIntelCustomStatusEntry latest = this.GetLatestCustomStatus();
            if (latest != null && latest.Text == text)
                return false;
            this.CustomStatusHistory.Add(new UserIntelCustomStatusEntry(text, DateTime.UtcNow));
            this.TrimCustomStatusHistory();
            return true;
        }

        public bool ChangeListeningStatus(SpotifyGame status)
        {
            UserIntelListeningStatusEntry latest = this.GetLatestListeningStatus();
            if (latest != null && latest.TrackID == status?.TrackId)
                return false;
            this.ListeningStatusHistory.Add(new UserIntelListeningStatusEntry(status));
            this.TrimListeningStatusHistory();
            return true;
        }

        private void TrimStatusHistory()
            => this.StatusHistory = this.TrimHistory(this.StatusHistory);
        private void TrimCustomStatusHistory()
            => this.CustomStatusHistory = this.TrimHistory(this.CustomStatusHistory);
        private void TrimListeningStatusHistory()
            => this.ListeningStatusHistory = this.TrimHistory(this.ListeningStatusHistory);

        private T GetLatestHistoryEntry<T>(IEnumerable<T> history) where T : IUserIntelHistoryEntry
            => history.OrderByDescending(entry => entry.Timestamp).FirstOrDefault();
        private ICollection<T> TrimHistory<T>(ICollection<T> history) where T : IUserIntelHistoryEntry
        {
            if (history.Count <= MaxHistorySize)
                return history;
            return history
                .OrderByDescending(status => status.Timestamp)
                .Take(MaxHistorySize)
                .ToList();
        }

        public ulong GetCacheKey()
            => this.ID;
    }
}
