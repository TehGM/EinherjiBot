using Discord;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Intel
{
    public class UserIntelListeningStatusEntry : IUserIntelHistoryEntry
    {
        [BsonElement("trackID")]
        public string TrackID { get; }
        [BsonElement("title"), BsonIgnoreIfNull]
        public string Title { get; }
        [BsonElement("album"), BsonIgnoreIfNull]
        public string Album { get; }
        [BsonElement("trackUrl"), BsonIgnoreIfNull]
        public string TrackURL { get; }
        [BsonElement("albumImageUrl"), BsonIgnoreIfNull]
        public string AlbumImageURL { get; }
        [BsonElement("artists"), BsonIgnoreIfDefault]
        public IEnumerable<string> Artists { get; }
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; }

        [BsonConstructor(nameof(TrackID), nameof(Title), nameof(Album), nameof(TrackURL), nameof(AlbumImageURL), nameof(Artists), nameof(Timestamp))]
        private UserIntelListeningStatusEntry(string trackID, string title, string album, string trackURL, string albumImageURL, IEnumerable<string> artists, DateTime timestamp)
        {
            this.TrackID = trackID;
            this.Title = title;
            this.Album = album;
            this.TrackURL = trackURL;
            this.AlbumImageURL = albumImageURL;
            this.Artists = artists?.ToArray();
            this.Timestamp = timestamp;
        }

        public UserIntelListeningStatusEntry(SpotifyGame status)
            : this(status?.TrackId, status?.TrackTitle, status?.AlbumTitle, status?.TrackUrl, status?.AlbumArtUrl, status?.Artists, status?.StartedAt?.UtcDateTime ?? DateTime.UtcNow) { }
    }
}
