using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.GameServers
{
    public class GameServer
    {
        [BsonId]
        public string Game { get; private set; }
        public string Address { get; private set; }
        public string Password { get; private set; }
        public string RulesURL { get; private set; }

        public bool IsPublic { get; private set; } = false;
        public HashSet<ulong> AuthorizedUserIDs { get; private set; }
        public HashSet<ulong> AuthorizedRoleIDs { get; private set; }

        [BsonConstructor]
        private GameServer()
        {
            this.AuthorizedUserIDs = new HashSet<ulong>();
            this.AuthorizedRoleIDs = new HashSet<ulong>();
        }
    }
}
