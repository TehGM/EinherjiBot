using MongoDB.Bson.Serialization.Attributes;
using System.Diagnostics;

namespace TehGM.EinherjiBot.BotStatus
{
    [DebuggerDisplay("{ToString(),nq} ({ID,nq})")]
    public class BotStatus : ICacheableEntity<Guid>, IBotStatus
    {
        [BsonId]
        public Guid ID { get; }
        [BsonElement("text")]
        public string Text { get; set; }
        [BsonElement("enabled")]
        public bool IsEnabled { get; set; }
        [BsonElement("lastError"), BsonIgnoreIfNull]
        public ErrorInfo LastError { get; set; }

        [BsonIgnore]
        IErrorInfo IBotStatus.LastError => this.LastError;

        [BsonConstructor]
        private BotStatus(Guid id)
        {
            this.ID = id;
        }

        public BotStatus(string text)
            : this(Guid.NewGuid())
        {
            this.Text = text;
            this.IsEnabled = true;
        }

        public override string ToString()
            => this.Text;

        public Guid GetCacheKey()
            => this.ID;
    }
}
