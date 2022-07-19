using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Intel
{
    public class UserIntelCustomStatusEntry : IUserIntelHistoryEntry
    {
        [BsonElement("text")]
        public string Text { get; }
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; }

        [BsonConstructor(nameof(Text), nameof(Timestamp))]
        public UserIntelCustomStatusEntry(string text, DateTime timestamp)
        {
            this.Text = text;
            this.Timestamp = timestamp;
        }
    }
}
