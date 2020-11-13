using System.Collections.Generic;
using Newtonsoft.Json;

namespace TehGM.EinherjiBot.DataMigration.Entities.Old
{
    class PatchbotHelperGame
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; private set; }
        [JsonProperty("aliases")]
        public List<string> Aliases { get; private set; }
        [JsonProperty("subscribersIds")]
        public HashSet<ulong> SubscribersIDs { get; private set; }
    }
}
