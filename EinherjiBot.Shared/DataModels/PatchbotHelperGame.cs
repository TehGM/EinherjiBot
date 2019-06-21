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
        public List<ulong> SubscribersIDs { get; private set; }

        public bool AddSubscriber(ulong id)
        {
            if (SubscribersIDs == null)
                SubscribersIDs = new List<ulong>();
            else if (SubscribersIDs.Contains(id))
                return false;
            SubscribersIDs.Add(id);
            return true;
        }

        public bool RemoveSubscriber(ulong id)
        {
            if (SubscribersIDs == null)
                return false;
            return SubscribersIDs.Remove(id);
        }
    }
}
