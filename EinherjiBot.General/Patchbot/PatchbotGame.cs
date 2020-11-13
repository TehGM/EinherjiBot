using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Patchbot
{
    public class PatchbotGame
    {
        [BsonId]
        public string Name { get; private set; }
        public HashSet<string> Aliases { get; private set; }
        public HashSet<ulong> SubscriberIDs { get; private set; }

        [BsonConstructor]
        private PatchbotGame()
        {
            this.Aliases = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            this.SubscriberIDs = new HashSet<ulong>();
        }

        public PatchbotGame(string name)
        {
            this.Name = name.Trim();
        }

        public PatchbotGame(string name, IEnumerable<string> aliases) : this(name)
        {
            this.Aliases = new HashSet<string>(aliases, StringComparer.OrdinalIgnoreCase);
        }

        public bool MatchesName(string name)
        {
            string trimmedName = name.Trim();
            return this.Name.Equals(trimmedName, StringComparison.OrdinalIgnoreCase)
                || this.Aliases.Contains(trimmedName);
        }
    }
}
