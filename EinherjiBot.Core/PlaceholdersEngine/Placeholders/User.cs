using Discord;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("User", PlaceholderUsage.Any)]
    public class UserPlaceholder
    {
        [PlaceholderProperty("ID", IsRequired = true, IDType = IDType.User)]
        public ulong UserID { get; init; }
        [PlaceholderProperty("Mode")]
        public UserDisplayMode DisplayMode { get; init; } = UserDisplayMode.Username;

        public class UserPlaceholderHandler : PlaceholderHandler<UserPlaceholder>
        {
            private readonly IDiscordEntityInfoService _provider;

            public UserPlaceholderHandler(IDiscordEntityInfoService provider)
            {
                this._provider = provider;
            }

            protected override async Task<string> GetReplacementAsync(UserPlaceholder placeholder, CancellationToken cancellationToken = default)
            {
                IDiscordUserInfo user = await this._provider.GetUserInfoAsync(placeholder.UserID, cancellationToken).ConfigureAwait(false);
                if (user == null)
                    throw new InvalidOperationException($"Discord user with ID {placeholder.UserID} not found");

                switch (placeholder.DisplayMode)
                {
                    case UserDisplayMode.Mention:
                        return MentionUtils.MentionUser(placeholder.UserID);
                    case UserDisplayMode.Username:
                        return user.Username;
                    case UserDisplayMode.UsernameWithDiscriminator:
                        return user.GetUsernameWithDiscriminator();
                    default:
                        throw new ArgumentException($"Unsupported display mode {placeholder.DisplayMode}", nameof(placeholder.DisplayMode));
                }
            }
        }
    }
}
