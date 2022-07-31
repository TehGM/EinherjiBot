using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.SharedAccounts
{
    public class SharedAccount : ICacheableEntity<Guid>
    {
        [BsonId]
        public Guid ID { get; }
        [BsonElement("type")]
        public SharedAccountType AccountType { get; }
        [BsonElement("login")]
        public string Login { get; set; }
        [BsonElement("password")]
        public string Password { get; set; }
        [BsonElement("allowedUsers")]
        public ICollection<ulong> AuthorizedUserIDs { get; }
        [BsonElement("allowedRoles")]
        public ICollection<ulong> AuthorizedRoleIDs { get; }
        [BsonElement("modUsers")]
        public ICollection<ulong> ModUserIDs { get; }

        [BsonElement("modifiedByID")]
        public ulong? ModifiedByID { get; set; }
        [BsonElement("modifiedTimestamp")]
        public DateTime? ModifiedTimestamp { get; set; }

        [BsonConstructor(nameof(ID), nameof(AccountType), nameof(AuthorizedUserIDs), nameof(AuthorizedRoleIDs), nameof(ModUserIDs))]
        private SharedAccount(Guid id, SharedAccountType accountType, IEnumerable<ulong> allowedUserIDs, IEnumerable<ulong> allowedRoleIDs, IEnumerable<ulong> modUserIDs)
        {
            this.ID = id;
            this.AccountType = accountType;
            this.AuthorizedUserIDs = new HashSet<ulong>(allowedUserIDs ?? Enumerable.Empty<ulong>());
            this.AuthorizedRoleIDs = new HashSet<ulong>(allowedRoleIDs ?? Enumerable.Empty<ulong>());
            this.ModUserIDs = new HashSet<ulong>(modUserIDs ?? Enumerable.Empty<ulong>());
        }

        public SharedAccount(SharedAccountType accountType, string login, string password)
            : this(Guid.NewGuid(), accountType, null, null, null)
        {
            this.Login = login;
            this.Password = password;
        }

        public Guid GetCacheKey()
            => this.ID;
    }
}
