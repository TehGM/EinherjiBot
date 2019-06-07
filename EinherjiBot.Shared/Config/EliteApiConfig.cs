using Discord;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TehGM.EinherjiBot.Config
{
    public class EliteApiConfig
    {
        [JsonProperty("eliteAutoNewsChannelId")]
        public ulong EliteAutoNewsChannelID { get; private set; }
        [JsonProperty("eliteAutoNewsIntervalSeconds")]
        private uint _eliteAutoNewsIntervalSeconds;
        [JsonProperty("preferPingOverPm")]
        public bool PreferPingOverPM { get; private set; }
        [JsonProperty("cgSubscribersIds")]
        public List<ulong> CommunityGoalsSubscribersIDs { get; private set; }
        [JsonIgnore]
        public TimeSpan EliteAutoNewsInterval
        {
            get { return TimeSpan.FromSeconds(_eliteAutoNewsIntervalSeconds); }
            set { _eliteAutoNewsIntervalSeconds = (uint)value.TotalSeconds; }
        }

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
