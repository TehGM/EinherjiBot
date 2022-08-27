using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace TehGM.EinherjiBot.SharedAccounts
{
    public class SharedAccountRequest : IValidatableObject
    {
        public const int LoginMaxLength = 64;
        public const int PasswordMaxLength = 128;

        [JsonProperty("type")]
        public SharedAccountType AccountType { get; init; }
        [JsonProperty("login", Required = Required.Always)]
        [Required, MaxLength(64)]
        public string Login { get; init; }
        [JsonProperty("password", Required = Required.AllowNull)]
        [MaxLength(64)]
        public string Password { get; init; }
        [JsonProperty("allowedUsers")]
        public ICollection<ulong> AuthorizedUserIDs { get; init; } = new List<ulong>();
        [JsonProperty("allowedRoles")]
        public ICollection<ulong> AuthorizedRoleIDs { get; init; } = new List<ulong>();
        [JsonProperty("modUsers")]
        public ICollection<ulong> ModUserIDs { get; init; } = new List<ulong>();

        [JsonConstructor]
        private SharedAccountRequest() { }

        public SharedAccountRequest(SharedAccountType accountType, string login, string password)
        {
            this.AccountType = accountType;
            this.Login = login;
            this.Password = password;
        }

        public SharedAccountRequest(ISharedAccount account)
            : this(account.AccountType, account.Login, account.Password)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));

            this.AuthorizedUserIDs = account.AuthorizedUserIDs != null ? new List<ulong>(account.AuthorizedUserIDs) : new List<ulong>();
            this.AuthorizedRoleIDs = account.AuthorizedRoleIDs != null ? new List<ulong>(account.AuthorizedRoleIDs) : new List<ulong>();
            this.ModUserIDs = account.AuthorizedUserIDs != null ? new List<ulong>(account.ModUserIDs) : new List<ulong>();
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            throw new NotImplementedException();
        }
    }
}
