using Discord;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Auditing
{
    public class CommandAuditEntry : BotAuditEntry
    {
        [BsonElement("channelID")]
        public ulong? ChannelID { get; }
        [BsonElement("guildID")]
        public ulong? GuildID { get; }
        [BsonElement("command")]
        public string Command { get; }
        [BsonElement("arguments"), BsonIgnoreIfNullAttribute, BsonIgnoreIfDefaultAttribute, BsonDefaultValueAttribute(null)]
        public IReadOnlyDictionary<string, object> Arguments { get; }

        [BsonConstructor(nameof(UserID), nameof(ChannelID), nameof(GuildID), nameof(Command), nameof(Arguments), nameof(Timestamp), nameof(ExpirationTimestamp))]
        private CommandAuditEntry(ulong? userID, ulong? channelID, ulong? guildID, string command, IDictionary<string, object> arguments, DateTime timestamp, TimeSpan? expiration)
            : base(userID, "Command", timestamp, expiration)
        {
            this.ChannelID = channelID;
            this.GuildID = guildID;
            this.Command = command;
            this.Arguments = new Dictionary<string, object>(arguments ?? Enumerable.Empty<KeyValuePair<string, object>>());
        }

        public CommandAuditEntry(IInteractionContext interaction, string command, IDictionary<string, object> arguments = null) 
            : this(interaction.User.Id, interaction.Channel?.Id, interaction.Guild?.Id, command, arguments, interaction.Interaction.CreatedAt.UtcDateTime, DefaultExpiration)
        {
        }
    }
}
