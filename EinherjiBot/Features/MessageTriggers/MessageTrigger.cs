using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.MessageTriggers
{
    public class MessageTrigger : ICacheableEntity<Guid>, IEquatable<MessageTrigger>
    {
        public const ulong GlobalGuildID = default;

        [BsonId]
        public Guid ID { get; }
        [BsonElement("guildID")]
        public ulong GuildID { get; }
        [BsonElement("channelIDs")]
        public ICollection<ulong> ChannelIDs { get; set; }
        [BsonElement("pattern")]
        public string Pattern { get; set; }
        [BsonElement("response")]
        public string Response { get; set; }

        [BsonElement("useRegex")]
        public bool IsRegex { get; set; }
        [BsonElement("ignoreCase")]
        public bool IgnoreCase { get; set; }
        [BsonElement("exactMatch")]
        public bool ExactMatch { get; set; }

        [BsonIgnore]
        private Lazy<Regex> PatternRegex { get; }
        [BsonIgnore]
        public bool IsGlobal => this.GuildID == GlobalGuildID;

        [BsonConstructor(nameof(ID), nameof(GuildID))]
        private MessageTrigger(Guid id, ulong guildID)
        {
            this.ID = id;
            this.GuildID = guildID;
            this.PatternRegex = new Lazy<Regex>(() => this.BuildPatternRegex());
        }

        public MessageTrigger(ulong guildID, string pattern, string response)
            : this(Guid.NewGuid(), guildID)
        {
            this.Pattern = pattern;
            this.Response = response;
            this.IgnoreCase = true;
        }

        private Regex BuildPatternRegex()
        {
            RegexOptions options = RegexOptions.CultureInvariant | RegexOptions.Singleline;
            if (this.IgnoreCase)
                options |= RegexOptions.IgnoreCase;
            string pattern = this.Pattern;
            if (!this.IsRegex)
                pattern = $"^{pattern.Replace("*", ".*")}$";
            return new Regex(pattern, options);
        }

        public bool IsMatch(string text)
        {
            StringComparison comparison = this.IgnoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
            if (this.ExactMatch)
                return text.Equals(this.Pattern, comparison);
            return this.PatternRegex.Value.IsMatch(text);
        }

        public Guid GetCacheKey()
            => this.ID;

        public override bool Equals(object obj)
            => this.Equals(obj as MessageTrigger);

        public bool Equals(MessageTrigger other)
            => other is not null && this.ID.Equals(other.ID);

        public override int GetHashCode()
            => HashCode.Combine(this.ID);
    }
}
