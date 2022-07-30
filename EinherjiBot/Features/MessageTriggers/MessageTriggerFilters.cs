using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.MessageTriggers
{
    public class MessageTriggerFilters
    {
        [BsonElement("whitelistedGuilds")]
        public ICollection<ulong> WhitelistedGuildIDs { get; set; }
        [BsonElement("blacklistedGuilds")]
        public ICollection<ulong> BlacklistedGuildIDs { get; set; }
        [BsonElement("whitelistedChannels")]
        public ICollection<ulong> WhitelistedChannelIDs { get; set; }
        [BsonElement("blacklistedChannels")]
        public ICollection<ulong> BlacklistedChannelIDs { get; set; }
        [BsonElement("whitelistedRoles")]
        public ICollection<ulong> WhitelistedRoleIDs { get; set; }
        [BsonElement("blacklistedRoles")]
        public ICollection<ulong> BlacklistedRoleIDs { get; set; }
        [BsonElement("whitelistedUsers")]
        public ICollection<ulong> WhitelistedUserIDs { get; set; }
        [BsonElement("blacklistedUsers")]
        public ICollection<ulong> BlacklistedUserIDs { get; set; }
    }
}
