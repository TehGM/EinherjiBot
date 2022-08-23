using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.SharedAccounts
{
    public class SharedAccount : ICacheableEntity<Guid>, ISharedAccount
    {
        [BsonId]
        public Guid ID { get; }
        [BsonElement("type")]
        public SharedAccountType AccountType { get; }
        [BsonElement("login")]
        public string Login { get; set; }
        [BsonElement("password")]
        public string Password { get; set; }
        [BsonElement("authorizedUsers")]
        public IEnumerable<ulong> AuthorizedUserIDs { get; set; }
        [BsonElement("authorizedRoles")]
        public IEnumerable<ulong> AuthorizedRoleIDs { get; set; }
        [BsonElement("modUsers")]
        public IEnumerable<ulong> ModUserIDs { get; set; }

        [BsonElement("modifiedBy")]
        public ulong? ModifiedByID { get; set; }
        [BsonElement("modifiedTimestamp")]
        public DateTime? ModifiedTimestamp { get; set; }

        [BsonConstructor(nameof(ID), nameof(AccountType), nameof(AuthorizedUserIDs), nameof(AuthorizedRoleIDs), nameof(ModUserIDs))]
        private SharedAccount(Guid id, SharedAccountType accountType, IEnumerable<ulong> authorizedUserIDs, IEnumerable<ulong> authorizedRoleIDs, IEnumerable<ulong> modUserIDs)
        {
            this.ID = id;
            this.AccountType = accountType;
            this.AuthorizedUserIDs = new HashSet<ulong>(authorizedUserIDs ?? Enumerable.Empty<ulong>());
            this.AuthorizedRoleIDs = new HashSet<ulong>(authorizedRoleIDs ?? Enumerable.Empty<ulong>());
            this.ModUserIDs = new HashSet<ulong>(modUserIDs ?? Enumerable.Empty<ulong>());
        }

        public SharedAccount(SharedAccountType accountType)
            : this(Guid.NewGuid(), accountType, null, null, null) { }

        public Guid GetCacheKey()
            => this.ID;
    }
}
