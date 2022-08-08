using Newtonsoft.Json;
using System.Diagnostics;

namespace TehGM.EinherjiBot.API
{
    [DebuggerDisplay("{ToString(),nq} ({ID,nq})")]
    public class RoleInfoResponse : IDiscordEntityInfo
    {
        public static RoleInfoResponse None { get; } = new RoleInfoResponse();

        [JsonProperty("id")]
        public ulong ID { get; init; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("guild")]
        public ulong GuildID { get; init; }
        [JsonProperty("guildName")]
        public string GuildName { get; init; }
        [JsonProperty("color")]
        public uint Color { get; init; }
        [JsonProperty("position")]
        public int Position { get; init; }

        [JsonConstructor]
        private RoleInfoResponse() { }

        public RoleInfoResponse(ulong id, string name, ulong guildID, string guildName, uint color = 0, int position = 0)
        {
            this.ID = id;
            this.Name = name;
            this.GuildID = guildID;
            this.GuildName = guildName;
            this.Color = color;
            this.Position = position;
        }

        public ulong GetCacheKey()
            => this.ID;

        public override string ToString()
            => this.Name;

        public override int GetHashCode()
            => HashCode.Combine(this.ID);
    }
}
