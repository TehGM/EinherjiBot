using System.Diagnostics;
using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.API;

namespace TehGM.EinherjiBot.UI.Security
{
    [DebuggerDisplay("{ToString(),nq} ({ID,nq})")]
    public class WebAuthContext : IAuthContext, IDiscordUserInfo, IEquatable<WebAuthContext>, IEquatable<IAuthContext>
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

        public override string ToString()
            => (this as IDiscordUserInfo).GetUsernameWithDiscriminator();

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
