using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace TehGM.EinherjiBot.SharedAccounts.API
{
    public class SharedAccountRequest
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

        public SharedAccountRequest(SharedAccountType accountType, string login, string password)
        {
            this.AccountType = accountType;
            this.Login = login;
            this.Password = password;
        }
    }
}
