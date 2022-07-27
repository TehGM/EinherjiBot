using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace TehGM.EinherjiBot.Security.API.Services
{
    public class JwtGenerator : IJwtGenerator
    {
        private readonly JwtOptions _options;
        private readonly JwtSecurityTokenHandler _handler;

        public JwtGenerator(IOptionsSnapshot<JwtOptions> options)
        {
            this._options = options.Value;
            this._handler = new JwtSecurityTokenHandler();
            this._handler.SetDefaultTimesOnTokenCreation = false;
        }

        public string Generate(UserSecurityData userData)
        {
            ClaimsIdentity subject = new ClaimsIdentity(JwtBearerDefaults.AuthenticationScheme, ClaimNames.UserID, ClaimNames.Roles);
            subject.AddClaim(new Claim(ClaimNames.UserID, userData.ID.ToString()));
            foreach (string role in userData.Roles)
                subject.AddClaim(new Claim(ClaimNames.Roles, role));

            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                Subject = subject,
                Audience = this._options.Audience,
                Expires = DateTime.UtcNow + this._options.Lifetime,
                SigningCredentials = new SigningCredentials(this._options.PrivateKey, "RS256"),
                Issuer = this._options.Issuer,
                NotBefore = DateTime.UtcNow,
                IssuedAt = DateTime.UtcNow
            };

            return this._handler.WriteToken(this._handler.CreateJwtSecurityToken(descriptor));
        }
    }
}
