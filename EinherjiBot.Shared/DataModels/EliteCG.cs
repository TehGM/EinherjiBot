using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TehGM.EinherjiBot.DataModels
{
    public sealed class EliteCG : IEquatable<EliteCG>
    {
        [JsonProperty("communitygoalName")]
        public string Name { get; private set; }
        [JsonProperty("starsystemName")]
        public string SystemName { get; private set; }
        [JsonProperty("stationName")]
        public string StationName { get; private set; }
        [JsonProperty("goalExpiry")]
        public DateTimeOffset ExpirationTime { get; private set; }
        [JsonProperty("tierReached")]
        public uint TierReached { get; private set; }
        [JsonProperty("tierMax")]
        public uint TierMax { get; private set; }
        [JsonProperty("contributorsNum")]
        public uint ContributingPilotsCount { get; private set; }
        [JsonProperty("contributionsTotal")]
        public uint ContributionsCount { get; private set; }
        [JsonProperty("isCompleted")]
        public bool IsCompleted { get; private set; }
        [JsonProperty("lastUpdate")]
        public DateTimeOffset LastUpdateTime { get; private set; }
        [JsonProperty("goalObjectiveText")]
        public string Objective { get; private set; }
        [JsonProperty("goalRewardText")]
        public string Reward { get; private set; }
        [JsonProperty("goalDescriptionText")]
        public string Description { get; private set; }
        [JsonProperty("inaraURL")]
        public string InaraURL { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj is EliteCG cg)
                return Equals(cg);
            return false;
        }

        public bool Equals(EliteCG other)
        {
            return Name == other.Name &&
                   ExpirationTime == other.ExpirationTime;
        }

        public override int GetHashCode()
        {
            var hashCode = 420877175;
            hashCode = hashCode * -1521134295 + Name.GetHashCode();
            hashCode = hashCode * -1521134295 + ExpirationTime.GetHashCode();
            return hashCode;
        }
    }
}
