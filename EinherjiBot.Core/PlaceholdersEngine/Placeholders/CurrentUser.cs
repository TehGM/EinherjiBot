using Discord;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("CurrentUser", PlaceholderUsage.AnyMessageContext | PlaceholderUsage.UserEvent)]
    [DisplayName("Current User")]
    [Description("Is replaced with mention/name of the user that sent the message.")]
    public class CurrentUserPlaceholder
    {
        [PlaceholderProperty("Mode")]
        [DisplayName("Display Mode")]
        [Description("Determines how the user will be displayed.")]
        public GuildUserDisplayMode DisplayMode { get; init; } = GuildUserDisplayMode.Nickname;
        [PlaceholderProperty("FallbackMode")]
        [DisplayName("Fallback Display Mode")]
        [Description("Determines how the user will be displayed if Nickname was not found. Ignored when default display mode isn't set to Nickname.")]
        public UserDisplayMode FallbackDisplayMode { get; init; } = UserDisplayMode.Username;

        public class CurrentUserPlaceholderHandler : PlaceholderHandler<CurrentUserPlaceholder>
        {
            private readonly PlaceholderConvertContext _context;
            private readonly IDiscordEntityInfoProvider _provider;

            public CurrentUserPlaceholderHandler(PlaceholderConvertContext context, IDiscordEntityInfoProvider provider)
            {
                this._context = context;
                this._provider = provider;
            }

            protected override async Task<string> GetReplacementAsync(CurrentUserPlaceholder placeholder, CancellationToken cancellationToken = default)
            {
                if (this._context.CurrentUserID == null)
                    throw new PlaceholderContextException($"{nameof(CurrentUserPlaceholder)} can only be used for user events.");

                IDiscordUserInfo user = this._context.CurrentGuildID != null
                    ? await this._provider.GetGuildUserInfoAsync(this._context.CurrentUserID.Value, this._context.CurrentGuildID.Value, cancellationToken).ConfigureAwait(false)
                    : await this._provider.GetUserInfoAsync(this._context.CurrentUserID.Value, cancellationToken).ConfigureAwait(false);

                GuildUserDisplayMode mode = placeholder.DisplayMode;

                if (mode == GuildUserDisplayMode.Nickname)
                {
                    GuildUserInfoResponse guildUser = user as GuildUserInfoResponse;
                    if (!string.IsNullOrWhiteSpace(guildUser?.Nickname))
                        return guildUser?.Nickname;
                    mode = (GuildUserDisplayMode)placeholder.FallbackDisplayMode;
                }

                if (mode == GuildUserDisplayMode.Mention)
                    return MentionUtils.MentionUser(user.ID);
                if (mode == GuildUserDisplayMode.Username)
                    return user.Username;
                if (mode == GuildUserDisplayMode.UsernameWithDiscriminator)
                    return user.GetUsernameWithDiscriminator();
                throw new ArgumentException($"Unsupported display mode {mode}", nameof(placeholder.DisplayMode));
            }
        }
    }
}
