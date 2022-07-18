using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;

namespace TehGM.EinherjiBot.EliteDangerous
{
    public sealed class CommunityGoal : IEquatable<CommunityGoal>, ICacheableEntity<int>
    {
        [BsonId]
        [JsonProperty("communitygoalGameID")]
        public int ID { get; private set; }
        [BsonElement("name")]
        [JsonProperty("communitygoalName")]
        public string Name { get; private set; }
        [BsonIgnore]
        [JsonProperty("starsystemName")]
        public string SystemName { get; private set; }
        [BsonIgnore]
        [JsonProperty("stationName")]
        public string StationName { get; private set; }
        [BsonElement("expirationTime")]
        [JsonProperty("goalExpiry")]
        public DateTimeOffset ExpirationTime { get; private set; }
        [BsonIgnore]
        [JsonProperty("tierReached")]
        public uint TierReached { get; private set; }
        [BsonIgnore]
        [JsonProperty("tierMax")]
        public uint TierMax { get; private set; }
        [BsonIgnore]
        [JsonProperty("contributorsNum")]
        public uint ContributingPilotsCount { get; private set; }
        [BsonIgnore]
        [JsonProperty("contributionsTotal")]
        public ulong ContributionsCount { get; private set; }
        [BsonElement("isCompleted")]
        [JsonProperty("isCompleted")]
        public bool IsCompleted { get; private set; }
        [BsonIgnore]
        [JsonProperty("lastUpdate")]
        public DateTimeOffset LastUpdateTime { get; private set; }
        [BsonIgnore]
        [JsonProperty("goalObjectiveText")]
        public string Objective { get; private set; }
        [BsonIgnore]
        [JsonProperty("goalRewardText")]
        public string Reward { get; private set; }
        [BsonIgnore]
        [JsonProperty("goalDescriptionText")]
        public string Description { get; private set; }
        [BsonIgnore]
        [JsonProperty("inaraURL")]
        public string InaraURL { get; private set; }

        public override bool Equals(object obj)
        {
            if (obj is CommunityGoal cg)
                return Equals(cg);
            return false;
        }

        public bool Equals(CommunityGoal other)
        {
            if (ID == 0 || other.ID == 0)
                return Name == other.Name && ExpirationTime == other.ExpirationTime;
            return this.ID == other.ID;
        }

        public int GetCacheKey()
            => this.ID;

        public override int GetHashCode()
        {
            if (ID == 0)
            {
                var hashCode = 420877175;
                hashCode = hashCode * -1521134295 + Name.GetHashCode();
                hashCode = hashCode * -1521134295 + ExpirationTime.GetHashCode();
                return hashCode;
            }
            return ID.GetHashCode();
        }
    }
}
