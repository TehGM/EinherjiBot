using Discord;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Settings
{
    public class JoinLeaveSettings : IJoinLeaveSettings
    {
        [BsonElement("enabled")]
        public bool IsEnabled { get; set; } = true;
        [BsonElement("useSystemChannel")]
        public bool UseSystemChannel { get; set; } = true;
        [BsonElement("channel")]
        public ulong? NotificationChannelID { get; set; }
        [BsonElement("message")]
        public string MessageTemplate { get; set; }
        [BsonElement("showUserAvatar")]
        public bool ShowUserAvatar { get; set; }
        [BsonIgnore]
        public Color EmbedColor { get; set; }
        [BsonElement("lastError"), BsonIgnoreIfNull]
        public ErrorInfo LastError { get; set; }

        [BsonElement("color")]
        public uint RawEmbedColor
        {
            get => this.EmbedColor;
            set => this.EmbedColor = value;
        }

        [BsonIgnore]
        IErrorInfo IJoinLeaveSettings.LastError => this.LastError;

        [BsonConstructor(nameof(MessageTemplate))]
        public JoinLeaveSettings(string messageTemplate)
        {
            this.MessageTemplate = messageTemplate;
        }
    }
}
