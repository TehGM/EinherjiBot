using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.SharedAccounts
{
    public class SharedAccountRequest : ICreateValidatable, IUpdateValidatable<ISharedAccount>, IValidatableObject
    {
        public const int LoginMaxLength = 64;
        public const int PasswordMaxLength = 128;

        [JsonProperty("type")]
        public SharedAccountType AccountType { get; init; }
        [JsonProperty("login", Required = Required.Always)]
        public string Login { get; init; }
        [JsonProperty("password", Required = Required.AllowNull)]
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

        public IEnumerable<string> ValidateForCreation()
            => this.ValidateShared();

        public IEnumerable<string> ValidateForUpdate(ISharedAccount existing)
        {
            if (existing.AccountType != this.AccountType)
                yield return "Cannot change type of an existing account.";

            foreach (string error in this.ValidateShared())
                yield return error;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
            => this.ValidateShared().Select(e => new ValidationResult(e));

        private IEnumerable<string> ValidateShared()
        {
            if (string.IsNullOrEmpty(this.Login))
                yield return $"{nameof(this.Login)} is required.";
            else if (this.Login.Length > LoginMaxLength)
                yield return $"{nameof(this.Login)} cannot be longer than {LoginMaxLength} characters.";

            if (this.Password != null && this.Password.Length > PasswordMaxLength)
                yield return $"{nameof(this.Password)} cannot be longer than {PasswordMaxLength} characters.";

            if (!this.ModUserIDs.IsSubsetOf(this.AuthorizedUserIDs))
                yield return $"Mod users must be a subset of authorized users.";
        }
    }
}
