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
        // web auth context will never have banned flag, as they'll simply not get authed by backend
        bool IAuthContext.IsBanned => false;
        string IDiscordEntityInfo.Name => this.Username;
        bool IDiscordUserInfo.IsBot => false;

        public IEnumerable<string> BotFeatures { get; }
        public IEnumerable<ulong> RecognizedDiscordGuildIDs { get; }
        public IEnumerable<ulong> RecognizedDiscordRoleIDs { get; }


        public WebAuthContext(ulong id, string username, string discriminator, string avatarHash, 
            IEnumerable<string> roles, IEnumerable<string> botFeatures, IEnumerable<ulong> recognizedGuilds, IEnumerable<ulong> recognizedRoles)
        {
            this.ID = id;
            this.Username = username;
            this.Discriminator = discriminator;
            this.AvatarHash = avatarHash;
            this.BotRoles = new HashSet<string>(roles ?? Enumerable.Empty<string>());
            this.BotFeatures = botFeatures;
            this.RecognizedDiscordGuildIDs = recognizedGuilds;
            this.RecognizedDiscordRoleIDs = recognizedRoles;
        }

        private WebAuthContext() { }

        public static WebAuthContext FromLoginResponse(LoginResponse response)
            => new WebAuthContext(response.User.ID, response.User.Username, response.User.Discriminator, response.User.AvatarHash, response.Roles, response.Features, response.RecognizedDiscordGuildIDs, response.RecognizedDiscordRoleIDs);

        public override string ToString()
            => this.GetUsernameWithDiscriminator();

        public override bool Equals(object obj)
            => this.Equals(obj as IAuthContext);
        public bool Equals(WebAuthContext other)
            => this.Equals(other as IAuthContext);
        public bool Equals(IAuthContext other)
            => other is not null && this.ID == other.ID;
        public override int GetHashCode()
            => HashCode.Combine(this.ID);

        public ulong GetCacheKey()
            => this.ID;

        public static bool operator ==(WebAuthContext left, WebAuthContext right)
            => EqualityComparer<WebAuthContext>.Default.Equals(left, right);
        public static bool operator !=(WebAuthContext left, WebAuthContext right)
            => !(left == right);
    }
}
