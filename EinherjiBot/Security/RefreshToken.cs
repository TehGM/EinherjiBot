using MongoDB.Bson.Serialization.Attributes;
using System.Diagnostics;

namespace TehGM.EinherjiBot.Security
{
    [DebuggerDisplay("{Token,nq}")]
    public class RefreshToken : IEquatable<RefreshToken>, IEquatable<string>
    {
        [BsonId]
        public string Token { get; }
        [BsonElement("userID")]
        public ulong UserID { get; }
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; }
        [BsonElement("expirationTimestamp")]
        public DateTime? ExpirationTimestamp { get; }
        [BsonElement("discordRefreshToken")]
        public string DiscordRefreshToken { get; }

        [BsonConstructor(nameof(Token), nameof(UserID), nameof(DiscordRefreshToken), nameof(Timestamp), nameof(ExpirationTimestamp))]
        public RefreshToken(string token, ulong userID, string discordRefreshToken, DateTime timestamp, DateTime? expirationTimestamp)
        {
            this.Token = token;
            this.UserID = userID;
            this.Timestamp = timestamp;
            this.ExpirationTimestamp = expirationTimestamp;
            this.DiscordRefreshToken = discordRefreshToken;
        }

        public override string ToString()
            => this.Token;
        public override bool Equals(object obj)
        {
            if (obj is RefreshToken token)
                return this.Equals(token);
            if (obj is string value)
                return this.Equals(value);
            return false;
        }
        public bool Equals(string other)
            => this.Token.Equals(other);
        public bool Equals(RefreshToken other)
            => other is not null && this.Token.Equals(other.Token);
        public override int GetHashCode()
            => HashCode.Combine(this.Token);
    }
}
