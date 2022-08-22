namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    public class CurrentUserPlaceholderHandler : PlaceholderHandler<CurrentUserPlaceholder>
    {
        private readonly IDiscordAuthContext _auth;

        public CurrentUserPlaceholderHandler(IDiscordAuthContext authContext)
        {
            this._auth = authContext;
        }

        protected override Task<string> GetReplacementAsync(CurrentUserPlaceholder placeholder, CancellationToken cancellationToken = default)
        {
            GuildUserDisplayMode mode = placeholder.DisplayMode;

            if (mode == GuildUserDisplayMode.Mention)
                return Task.FromResult(this._auth.DiscordUser.Mention);

            if (mode == GuildUserDisplayMode.Nickname)
            {
                if (!string.IsNullOrWhiteSpace(this._auth.DiscordGuildUser?.Nickname))
                    return Task.FromResult(this._auth.DiscordGuildUser.Nickname);
                mode = (GuildUserDisplayMode)placeholder.FallbackDisplayMode;
            }

            if (mode == GuildUserDisplayMode.Username)
                return Task.FromResult(this._auth.DiscordUser.Username);
            if (mode == GuildUserDisplayMode.UsernameWithDiscriminator)
                return Task.FromResult(this._auth.DiscordUser.GetUsernameWithDiscriminator());
            throw new ArgumentException($"Unsupported display mode {mode}", nameof(placeholder.DisplayMode));
        }
    }
}
