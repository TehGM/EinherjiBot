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
        /// <summary>Primary role color. 0 if role has no color.</summary>
        [JsonProperty("color")]
        public uint Color { get; init; }
        /// <summary>Secondary role color. Only set if role uses gradient colors.</summary>
        [JsonProperty("secondaryColor")]
        public uint? SecondaryColor { get; init; }
        /// <summary>Tertiary role color. Only set if role uses holographic colors.</summary>
        [JsonProperty("tertiaryColor")]
        public uint? TertiaryColor { get; init; }
        [JsonProperty("position")]
        public int Position { get; init; }

        [JsonIgnore]
        public bool IsEveryone => this.Position == 0;
        [JsonIgnore]
        public bool IsGradient => this.Color != 0 && this.SecondaryColor != null;

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
