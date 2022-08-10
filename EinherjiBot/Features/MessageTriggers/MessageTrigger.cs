﻿using MongoDB.Bson.Serialization.Attributes;
using TehGM.EinherjiBot.PlaceholdersEngine;

namespace TehGM.EinherjiBot.MessageTriggers
{
    public class MessageTrigger : ICacheableEntity<Guid>, IEquatable<MessageTrigger>
    {
        public const ulong GlobalGuildID = default;

        [BsonId]
        public Guid ID { get; }
        [BsonElement("guildID")]
        public ulong GuildID { get; }
        [BsonElement("filters")]
        public MessageTriggerFilters Filters { get; }
        [BsonElement("actions")]
        public ICollection<IMessageTriggerAction> Actions { get; }

        [BsonElement("pattern")]
        public string Pattern { get; set; }
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
        [BsonIgnore]
        public PlaceholderUsage PlaceholderContext => this.IsGlobal ? PlaceholderUsage.GlobalMessageTrigger : PlaceholderUsage.MessageTrigger;

        [BsonConstructor(nameof(ID), nameof(GuildID), nameof(Filters), nameof(Actions))]
        private MessageTrigger(Guid id, ulong guildID, MessageTriggerFilters filters, IEnumerable<IMessageTriggerAction> actions)
        {
            this.ID = id;
            this.GuildID = guildID;
            this.Filters = filters;
            this.PatternRegex = new Lazy<Regex>(() => this.BuildPatternRegex());
            this.Actions = actions as ICollection<IMessageTriggerAction> ?? new List<IMessageTriggerAction>(actions ?? Enumerable.Empty<IMessageTriggerAction>());
        }

        public MessageTrigger(ulong guildID, string pattern, IEnumerable<IMessageTriggerAction> actions)
            : this(Guid.NewGuid(), guildID, new MessageTriggerFilters(), actions)
        {
            this.Pattern = pattern;
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
