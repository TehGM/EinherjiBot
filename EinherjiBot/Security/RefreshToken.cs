using MongoDB.Bson.Serialization.Attributes;
using System.Diagnostics;
using System.Security.Cryptography;

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
        private RefreshToken(string token, ulong userID, string discordRefreshToken, DateTime timestamp, DateTime? expirationTimestamp)
        {
            this.Token = token;
            this.UserID = userID;
            this.Timestamp = timestamp;
            this.ExpirationTimestamp = expirationTimestamp;
            this.DiscordRefreshToken = discordRefreshToken;
        }

        public static RefreshToken Create(ulong userID, string discordRefreshToken, TimeSpan? lifetime)
        {
            if (string.IsNullOrWhiteSpace(discordRefreshToken))
                throw new ArgumentNullException(nameof(discordRefreshToken));

            DateTime timestamp = DateTime.UtcNow;
            DateTime? expirationTimestamp = lifetime != null ? timestamp + lifetime : null;
            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)).TrimEnd('=');

            return new RefreshToken(token, userID, discordRefreshToken, timestamp, expirationTimestamp);
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
