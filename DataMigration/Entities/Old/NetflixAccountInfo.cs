using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace TehGM.EinherjiBot.DataMigration.Entities.Old
{
    class NetflixAccountInfo
    {
        [JsonProperty("login")]
        public string Login { get; private set; }
        [JsonProperty("password")]
        public string Password { get; private set; }
        [JsonProperty("modifiedUserId")]
        public ulong LastModifiedByID { get; private set; }
        [JsonProperty("modifiedTime")]
        public DateTimeOffset LastModifiedTimeUtc { get; private set; }
        [JsonProperty("retrieveRolesIds")]
        public HashSet<ulong> RetrieveRolesID { get; private set; }
        [JsonProperty("modifyUsersIds")]
        public HashSet<ulong> ModifyUsersIDs { get; private set; }
        [JsonProperty("allowedChannelsIds")]
        public HashSet<ulong> AllowedChannelsIDs { get; private set; }
        [JsonProperty("autoremoveDelaySecs")]
        public int AutoremoveDelay { get; private set; }
    }
}
