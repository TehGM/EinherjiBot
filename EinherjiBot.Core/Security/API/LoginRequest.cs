using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace TehGM.EinherjiBot.Security.API
{
    public class LoginRequest
    {
        [JsonProperty("discordCode")]
        [Required]
        public string DiscordCode { get; init; }

        [JsonConstructor]
        private LoginRequest() { }

        public LoginRequest(string discordCode)
        {
            this.DiscordCode = discordCode;
        }
    }
}
