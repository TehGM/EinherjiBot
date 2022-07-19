using Discord;

namespace TehGM.EinherjiBot.Security
{
    public class DiscordSocketUserContext : IUserContext
    {
        public ulong ID => this._user.Id;
        public string DisplayName => this._user.GetUsernameWithDiscriminator();
        public string AvatarURL => this._user.GetMaxAvatarUrl();

        public IEnumerable<string> Roles => this._data.Roles;

        private readonly IUser _user;
        private readonly UserSecurityData _data;

        public DiscordSocketUserContext(IUser user, UserSecurityData data)
        {
            this._user = user;
            this._data = data;
        }
    }
}
