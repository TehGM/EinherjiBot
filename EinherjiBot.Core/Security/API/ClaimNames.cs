using System.IdentityModel.Tokens.Jwt;

namespace TehGM.EinherjiBot.Security.API
{
    public class ClaimNames
    {
        public const string Subject = JwtRegisteredClaimNames.Sub;
        public const string UserID = Subject;
        public const string Roles = "roles";
        public const string Scope = "scope";
    }
}
