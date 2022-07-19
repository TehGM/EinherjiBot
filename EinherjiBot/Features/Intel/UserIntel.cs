using Discord;

namespace TehGM.EinherjiBot.Intel
{
    public class UserIntel
    {
        public ulong ID => this.User.Id;

        public IUser User { get; }
        public IGuildUser GuildUser { get; }
        public UserOnlineHistory StatusHistory { get; }

        public UserOnlineHistoryEntry LatestStatus { get; }

        public UserIntel(IUser user, IGuildUser guildUser, UserOnlineHistory history)
        {
            this.User = user ?? throw new ArgumentNullException(nameof(user));
            this.GuildUser = guildUser;
            this.StatusHistory = history ?? throw new ArgumentNullException(nameof(history));

            // we don't want to report the status update if it's mismatched
            // while that's not very likely, due to disconnections presence updates might be missed at times
            UserOnlineHistoryEntry latestStatus = this.StatusHistory.GetLatestStatus();
            if (latestStatus?.MatchesStatus(this.User.Status) == true)
                this.LatestStatus = latestStatus;
        }

        public UserIntel(IUser user, UserOnlineHistory history)
            : this(user, null, history) { }
    }
}
