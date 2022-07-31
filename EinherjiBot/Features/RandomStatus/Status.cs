using Discord;
using MongoDB.Bson.Serialization.Attributes;
using System.Diagnostics;

namespace TehGM.EinherjiBot.RandomStatus
{
    [DebuggerDisplay("{ToString(),nq} ({ID,nq})")]
    public class Status : ICacheableEntity<Guid>
    {
        [BsonId]
        public Guid ID { get; }
        [BsonElement("text")]
        public string Text { get; set; }
        [BsonElement("link")]
        public string Link { get; set; }
        [BsonElement("activity")]
        public ActivityType ActivityType { get; set; } = ActivityType.Playing;
        [BsonElement("enabled")]
        public bool IsEnabled { get; set; }

        [BsonConstructor]
        private Status(Guid id)
        {
            this.ID = id;
        }

        public Status(string text, string link, ActivityType activityType = ActivityType.Playing)
            : this(Guid.NewGuid())
        {
            this.Text = text;
            this.Link = link;
            this.ActivityType = activityType;
            this.IsEnabled = true;
        }

        public Status(string text, ActivityType activityType = ActivityType.Playing)
            : this(text, null, activityType) { }

        public override string ToString()
        {
            switch (this.ActivityType)
            {
                case ActivityType.Playing:
                    return $"Playing {this.Text}";
                case ActivityType.Streaming:
                    return $"Streaming {this.Text}";
                case ActivityType.Watching:
                    return $"Watching {this.Text}";
                case ActivityType.Listening:
                    return $"Listening to {this.Text}";
                default:
                    throw new NotSupportedException($"Activity of type {this.ActivityType} is not supported");
            }
        }

        public Guid GetCacheKey()
            => this.ID;
    }
}
