using Newtonsoft.Json;

namespace TehGM.EinherjiBot.SharedAccounts
{
    public class SharedAccountResponse : ICacheableEntity<Guid>, ISharedAccount
    {
        [JsonProperty("id")]
        public Guid ID { get; init; }
        [JsonProperty("type")]
        public SharedAccountType AccountType { get; init; }
        [JsonProperty("login")]
        public string Login { get; init; }
        [JsonProperty("password")]
        public string Password { get; init; }
        [JsonProperty("image")]
        public string ImageURL { get; init; }
        [JsonProperty("allowedUsers", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ulong> AuthorizedUserIDs { get; init; }
        [JsonProperty("allowedRoles", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ulong> AuthorizedRoleIDs { get; init; }
        [JsonProperty("modUsers", NullValueHandling = NullValueHandling.Ignore)]
        public IEnumerable<ulong> ModUserIDs { get; init; }

        [JsonProperty("modifiedBy")]
        public ulong? ModifiedByID { get; init; }
        [JsonProperty("modifiedTimestamp")]
        public DateTime? ModifiedTimestamp { get; init; }

        [JsonConstructor]
        private SharedAccountResponse() { }

        public SharedAccountResponse(Guid id, SharedAccountType accountType, string login, string password)
        {
            this.ID = id;
            this.AccountType = accountType;
            this.Login = login;
            this.Password = password;
        }

        public Guid GetCacheKey()
            => this.ID;
    }
}
