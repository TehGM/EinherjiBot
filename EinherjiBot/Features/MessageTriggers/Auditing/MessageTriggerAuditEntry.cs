using Discord;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Auditing.MessageTriggers
{
    public class MessageTriggerAuditEntry : BotAuditEntry
    {
        [BsonElement("channelID")]
        public ulong? ChannelID { get; }
        [BsonElement("guildID")]
        public ulong? GuildID { get; }
        [BsonElement("text")]
        public string Text { get; }
        [BsonElement("attachments"), BsonIgnoreIfDefault, BsonIgnoreIfNull, BsonDefaultValue(null)]
        public IEnumerable<string> AttachmentURLs { get; }

        [BsonConstructor(nameof(UserID), nameof(ChannelID), nameof(GuildID), nameof(Text), nameof(AttachmentURLs), nameof(Timestamp), nameof(ExpirationTimestamp))]
        private MessageTriggerAuditEntry(ulong? userID, ulong? channelID, ulong? guildID, string text, IEnumerable<string> attachmentUrls, DateTime timestamp, DateTime? expirationTimestamp)
            : base(userID, "MessageTrigger", timestamp, expirationTimestamp)
        {
            this.ChannelID = channelID;
            this.GuildID = guildID;
            this.Text = text;
            if (attachmentUrls?.Any() == true)
                this.AttachmentURLs = attachmentUrls;
        }

        public MessageTriggerAuditEntry(IMessage message, string text, TimeSpan? lifetime)
            : this(message.Author.Id, message.Channel?.Id, (message.Channel as IGuildChannel)?.Guild?.Id, text,
                  message.Attachments.Select(a => a.Url),
                  message.Timestamp.UtcDateTime, message.Timestamp.UtcDateTime + (lifetime ?? DefaultExpiration)) { }
    }
}
