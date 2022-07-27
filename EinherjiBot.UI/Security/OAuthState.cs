using Newtonsoft.Json;
using System.Text;

namespace TehGM.EinherjiBot.UI.Security
{
    public class OAuthState
    {
        [JsonProperty("id")]
        public Guid ID { get; init; }
        [JsonProperty("returnUrl", DefaultValueHandling = DefaultValueHandling.Ignore, NullValueHandling = NullValueHandling.Ignore)]
        public string ReturnURL { get; init; }

        [JsonConstructor]
        public OAuthState()
        {
            this.ID = Guid.NewGuid();
        }

        public OAuthState(string returnUrl)
            : this()
        {
            this.ReturnURL = returnUrl;
        }

        public override string ToString()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.None);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }

        public static OAuthState Parse(string encoded)
        {
            byte[] bytes = Convert.FromBase64String(encoded);
            string json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<OAuthState>(json);
        }
    }
}
