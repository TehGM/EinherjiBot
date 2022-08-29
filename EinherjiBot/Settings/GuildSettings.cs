using Discord;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Settings
{
    public class GuildSettings : IGuildSettings, ICacheableEntity<ulong>
    {
        [BsonId]
        public ulong GuildID { get; }
        [BsonElement("joinNotificationChannel")]
        public ulong? JoinNotificationChannelID { get; set; }
        [BsonElement("leaveNotificationChannel")]
        public ulong? LeaveNotificationChannelID { get; set; }
        [BsonElement("maxMessageTriggers")]
        public uint? MaxMessageTriggers { get; set; } = 7;

        [BsonConstructor(nameof(GuildID))]
        public GuildSettings(ulong guildID)
        {
            this.GuildID = guildID;
        }

        public static GuildSettings CreateDefault(IGuild guild)
        {
            return new GuildSettings(guild.Id)
            {
                JoinNotificationChannelID = guild.SystemChannelId,
                LeaveNotificationChannelID = guild.SystemChannelId
            };
        }

        public ulong GetCacheKey()
            => this.GuildID;
    }
}
