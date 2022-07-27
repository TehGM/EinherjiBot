using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace TehGM.EinherjiBot.Security.API
{
    public class RefreshRequest
    {
        [JsonProperty("refreshToken")]
        [Required]
        public string RefreshToken { get; init; }

        [JsonConstructor]
        private RefreshRequest() { }

        public RefreshRequest(string refreshToken)
        {
            this.RefreshToken = refreshToken;
        }
    }
}
