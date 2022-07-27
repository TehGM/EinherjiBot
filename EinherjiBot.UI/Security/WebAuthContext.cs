using System.Diagnostics;
using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.API;

namespace TehGM.EinherjiBot.UI.Security
{
    [DebuggerDisplay("{ToString(),nq} ({ID,nq})")]
    public class WebAuthContext : IAuthContext, IEquatable<WebAuthContext>, IEquatable<IAuthContext>
    {
        public static WebAuthContext None { get; } = new WebAuthContext();

        public ulong ID { get; }
        public string Username { get; }
        public string Discriminator { get; }
        public string AvatarHash { get; }
        public IEnumerable<string> BotRoles { get; }

        public WebAuthContext(ulong id, string username, string discriminator, string avatarHash, IEnumerable<string> roles)
        {
            this.ID = id;
            this.Username = username;
            this.Discriminator = discriminator;
            this.AvatarHash = avatarHash;
            this.BotRoles = new HashSet<string>(roles ?? Enumerable.Empty<string>());
        }

        private WebAuthContext() { }

        public static WebAuthContext FromLoginResponse(LoginResponse response)
            => new WebAuthContext(response.User.ID, response.User.Username, response.User.Discriminator, response.User.AvatarHash, response.Roles);

        public string GetAvatarURL(ushort size = 1024)
        {
            const string baseUrl = "https://cdn.discordapp.com";
            if (string.IsNullOrWhiteSpace(this.AvatarHash))
            {
                int value = int.Parse(this.Discriminator) % 5;
                return $"{baseUrl}/embed/avatars/{value}.png";
            }

            string ext = this.AvatarHash.StartsWith("a_", StringComparison.Ordinal) ? "gif" : "png";
            return $"{baseUrl}/avatars/{this.ID}/{this.AvatarHash}.{ext}?size={size}";
        }

        public override string ToString()
            => $"{this.Username}#{this.Discriminator}";

        public override bool Equals(object obj)
            => this.Equals(obj as IAuthContext);
        public bool Equals(WebAuthContext other)
            => this.Equals(other as IAuthContext);
        public bool Equals(IAuthContext other)
            => other is not null && this.ID == other.ID;
        public override int GetHashCode()
            => HashCode.Combine(this.ID);
        public static bool operator ==(WebAuthContext left, WebAuthContext right)
            => EqualityComparer<WebAuthContext>.Default.Equals(left, right);
        public static bool operator !=(WebAuthContext left, WebAuthContext right)
            => !(left == right);
    }
}
