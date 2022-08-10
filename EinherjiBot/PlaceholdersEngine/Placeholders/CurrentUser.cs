namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [OldPlaceholder($"{{{{CurrentUser(?::({_modeMention}|{_modeNickname}|{_modeUsername}|{_modeUsernameWithDiscriminator}))?(?::({_modeMention}|{_modeUsername}|{_modeUsernameWithDiscriminator}))?}}}}",
        DisplayName = "{{CurrentUser}}")]
    public class CurrentUser : IPlaceholder
    {
        private const string _modeMention = "Mention";
        private const string _modeNickname = "Nickname";
        private const string _modeUsername = "Username";
        private const string _modeUsernameWithDiscriminator = "UsernameWithDiscriminator";
        private const string _defaultMode = _modeNickname;
        private const string _defaultFallbackMode = _modeUsername;

        private readonly IDiscordAuthContext _auth;

        public CurrentUser(IDiscordAuthContext authContext)
        {
            this._auth = authContext;
        }

        public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            string mode = _defaultMode;
            if (placeholder.Groups[1].Success)
                mode = placeholder.Groups[1].Value;

            if (mode == _modeMention)
                return Task.FromResult(this._auth.DiscordUser.Mention);

            if (mode == _modeNickname)
            {
                if (!string.IsNullOrWhiteSpace(this._auth.DiscordGuildUser?.Nickname))
                    return Task.FromResult(this._auth.DiscordGuildUser.Nickname);
                mode = _defaultFallbackMode;
                if (placeholder.Groups[2].Success)
                    mode = placeholder.Groups[2].Value;
            }

            if (mode == _modeUsername)
                return Task.FromResult(this._auth.DiscordUser.Username);
            else
                return Task.FromResult(this._auth.DiscordUser.GetUsernameWithDiscriminator());
        }
    }
}
