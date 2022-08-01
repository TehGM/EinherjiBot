using Newtonsoft.Json;
using System.Diagnostics;
using System.Web;

namespace TehGM.EinherjiBot.API
{
    [DebuggerDisplay("{ToString(),nq} ({ID,nq})")]
    public class UserGuildInfoResponse
    {
        [JsonProperty("id")]
        public ulong ID { get; init; }
        [JsonProperty("name")]
        public string Name { get; init; }
        [JsonProperty("icon")]
        public string IconHash { get; init; }
        [JsonProperty("owner")]
        public bool IsOwner { get; init; }
        [JsonProperty("permissions")]
        public ulong PermissionFlags { get; init; }
        [JsonProperty("features")]
        public IEnumerable<string> EnabledFeatures { get; init; }

        [JsonIgnore]
        public bool CanManage
            => this.IsOwner
            || (this.PermissionFlags & 0x8uL) == 0x8uL;  // admin

        [JsonConstructor]
        private UserGuildInfoResponse() { }

        public override string ToString()
            => this.Name;

        public string GetIconURL(ushort size = 1024)
        {
            if (string.IsNullOrWhiteSpace(this.IconHash))
            {
                string encodedName = HttpUtility.UrlEncodeUnicode(this.Name);
                return $"https://ui-avatars.com/api?name={encodedName}&size={size}&length=3&uppercase=false&format=png";
            }
            string ext = this.IconHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png";
            return $"https://cdn.discordapp.com/icons/{this.ID}/{this.IconHash}.{ext}?size={size}";
        }
    }
}
