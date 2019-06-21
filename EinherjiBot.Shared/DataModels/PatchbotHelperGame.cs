using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TehGM.EinherjiBot.DataModels
{
    public class PatchbotHelperGame
    {
        [JsonProperty("name", Required = Required.Always)]
        public string Name { get; private set; }
        [JsonProperty("aliases")]
        public string[] Aliases { get; private set; }
        [JsonProperty("subscribersIds")]
        public HashSet<ulong> SubscribersIDs { get; private set; }

        public bool AddSubscriber(ulong id)
        {
            if (SubscribersIDs == null)
                SubscribersIDs = new HashSet<ulong>();
            return SubscribersIDs.Add(id);
        }

        public bool RemoveSubscriber(ulong id)
        {
            if (SubscribersIDs == null)
                return false;
            return SubscribersIDs.Remove(id);
        }
    }
}
