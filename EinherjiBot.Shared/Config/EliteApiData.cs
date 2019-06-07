using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TehGM.EinherjiBot.Config
{
    public class EliteApiData
    {
        [JsonProperty("cgSubscribersIds")]
        public List<ulong> CommunityGoalsSubscribersIDs { get; private set; }
        [JsonProperty("autoNewsRetrievalTimeUtc")]
        public DateTime AutoNewsRetrievalTimeUtc { get; set; }

        public bool AddCommunityGoalsSubscriber(ulong userID)
        {
            if (CommunityGoalsSubscribersIDs.Contains(userID))
                return false;
            CommunityGoalsSubscribersIDs.Add(userID);
            return true;
        }
        public bool AddCommunityGoalsSubscriber(IUser user)
            => AddCommunityGoalsSubscriber(user.Id);

        public bool RemoveCommunityGoalsSubscriber(ulong userID)
            => CommunityGoalsSubscribersIDs.Remove(userID);
        public bool RemoveCommunityGoalsSubscriber(IUser user)
            => RemoveCommunityGoalsSubscriber(user.Id);
    }
}
