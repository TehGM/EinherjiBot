using System;
using Newtonsoft.Json;

namespace TehGM.EinherjiBot.DataMigration.Entities.Old
{
    class UserIntel
    {
        [JsonProperty("userId", Required = Required.Always)]
        public ulong UserID { get; }
        [JsonProperty("isOnline")]
        public bool IsOnline { get; private set; }
        [JsonProperty("changeTimeUtc")]
        public DateTime? ChangeTimeUTC { get; private set; }

        public UserIntel(ulong userId)
        {
            this.UserID = userId;
        }
    }
}
