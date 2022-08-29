using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Settings
{
    public class GuildSettings : IGuildSettings, ICacheableEntity<ulong>
    {
        [BsonId]
        public ulong GuildID { get; }
        [BsonElement("joinNotificationChannel")]
        public JoinLeaveSettings JoinNotification { get; set; }
        [BsonElement("leaveNotificationChannel")]
        public JoinLeaveSettings LeaveNotification { get; set; }
        [BsonElement("maxMessageTriggers")]
        public uint? MaxMessageTriggers { get; set; } = 7;

        IJoinLeaveSettings IGuildSettings.JoinNotification => this.JoinNotification;
        IJoinLeaveSettings IGuildSettings.LeaveNotification => this.LeaveNotification;

        [BsonConstructor(nameof(GuildID))]
        public GuildSettings(ulong guildID)
        {
            this.GuildID = guildID;
        }

        public ulong GetCacheKey()
            => this.GuildID;
    }
}
