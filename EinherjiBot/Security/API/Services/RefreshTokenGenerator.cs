using System.Security.Cryptography;

namespace TehGM.EinherjiBot.Security.API.Services
{
    public class RefreshTokenGenerator : IRefreshTokenGenerator
    {
        private readonly RefreshTokenOptions _options;

        public RefreshTokenGenerator(IOptionsSnapshot<RefreshTokenOptions> options)
        {
            this._options = options.Value;
        }

        public RefreshToken Generate(ulong userID, string discordRefreshToken)
        {
            if (string.IsNullOrWhiteSpace(discordRefreshToken))
                throw new ArgumentNullException(nameof(discordRefreshToken));

            DateTime timestamp = DateTime.UtcNow;
            DateTime? expirationTimestamp = this._options.Lifetime != null ? timestamp + this._options.Lifetime : null;
            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(this._options.BytesLength)).TrimEnd('=');

            return new RefreshToken(token, userID, discordRefreshToken, timestamp, expirationTimestamp);
        }
    }
}
