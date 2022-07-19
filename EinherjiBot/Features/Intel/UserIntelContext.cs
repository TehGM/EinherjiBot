using Discord;

namespace TehGM.EinherjiBot.Intel
{
    public class UserIntelContext
    {
        public ulong ID => this.User.Id;

        public IUser User { get; }
        public IGuildUser GuildUser { get; }
        public UserIntel Intel { get; }

        public UserIntelStatusEntry LatestStatus { get; }

        public UserIntelContext(IUser user, IGuildUser guildUser, UserIntel history)
        {
            this.User = user ?? throw new ArgumentNullException(nameof(user));
            this.GuildUser = guildUser;
            this.Intel = history ?? throw new ArgumentNullException(nameof(history));

            // we don't want to report the status update if it's mismatched
            // while that's not very likely, due to disconnections presence updates might be missed at times
            UserIntelStatusEntry latestStatus = this.Intel.GetLatestStatus();
            if (latestStatus?.MatchesStatus(this.User.Status) == true)
                this.LatestStatus = latestStatus;
        }

        public UserIntelContext(IUser user, UserIntel history)
            : this(user, null, history) { }
    }
}
