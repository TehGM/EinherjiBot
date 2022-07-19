using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Security
{
    public class UserSecurityData : ICacheableEntity<ulong>
    {
        [BsonId]
        public ulong ID { get; }
        [BsonElement("roles")]
        public ICollection<string> Roles { get; }
        [BsonElement("banned")]
        public bool IsBanned { get; set; }

        [BsonConstructor(nameof(ID), nameof(Roles))]
        private UserSecurityData(ulong id, IEnumerable<string> roles)
        {
            this.ID = id;
            this.Roles = roles?.ToHashSet() ?? new HashSet<string>();
        }

        public UserSecurityData(ulong id)
            : this(id, null) { }

        public ulong GetCacheKey()
            => this.ID;
    }
}
