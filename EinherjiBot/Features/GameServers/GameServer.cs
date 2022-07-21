using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.GameServers
{
    public class GameServer : ICacheableEntity<Guid>
    {
        [BsonId]
        public Guid ID { get; }
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("address")]
        public string Address { get; set; }
        [BsonElement("password")]
        public string Password { get; set; }
        [BsonElement("rulesUrl")]
        public string RulesURL { get; set; }
        [BsonElement("isPublic")]
        public bool IsPublic { get; set; }
        [BsonElement("imageUrl")]
        public string ImageURL { get; set; }

        [BsonElement("authorizedUserIDs")]
        public HashSet<ulong> AuthorizedUserIDs { get; }
        [BsonElement("authorizedRoleIDs")]
        public HashSet<ulong> AuthorizedRoleIDs { get; }

        [BsonConstructor(nameof(ID), nameof(AuthorizedUserIDs), nameof(AuthorizedRoleIDs))]
        private GameServer(Guid id, IEnumerable<ulong> userIDs, IEnumerable<ulong> roleIDs)
        {
            this.ID = id;
            this.AuthorizedUserIDs = new HashSet<ulong>(userIDs ?? Enumerable.Empty<ulong>());
            this.AuthorizedRoleIDs = new HashSet<ulong>(roleIDs ?? Enumerable.Empty<ulong>());
        }

        public GameServer(string name, string address, string password)
            : this(Guid.NewGuid(), null, null)
        {
            this.Name = name;
            this.Address = address;
            this.Password = password;
        }

        public Guid GetCacheKey()
            => this.ID;
    }
}
