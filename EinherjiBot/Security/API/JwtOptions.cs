using Microsoft.IdentityModel.Tokens;

namespace TehGM.EinherjiBot.Security.API
{
    public class JwtOptions
    {
        public TimeSpan Lifetime { get; set; } = TimeSpan.FromMinutes(5);

        public string Audience { get; set; } = EinherjiInfo.WebsiteURL;
        public string Issuer { get; set; } = EinherjiInfo.WebsiteURL + "/api";

        public string PrivateKeyBase64 { get; set; }
        public string PublicKeyBase64 { get; set; }
        public AsymmetricSecurityKey PrivateKey { get; set; }
        public AsymmetricSecurityKey PublicKey { get; set; }
    }
}
