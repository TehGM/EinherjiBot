namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder($"{{{{CurrentUserName(?::({_modeNickname}|{_modeUsername}|{_modeUsernameWithDiscriminator}))?(?::({_modeNickname}|{_modeUsername}|{_modeUsernameWithDiscriminator}))?}}}}")]
    public class CurrentUserName : IPlaceholder
    {
        private const string _modeNickname = "Nickname";
        private const string _modeUsername = "Username";
        private const string _modeUsernameWithDiscriminator = "UsernameWithDiscriminator";
        private const string _defaultMode = _modeNickname;
        private const string _defaultFallbackMode = _modeUsername;

        private readonly IDiscordAuthContext _auth;

        public CurrentUserName(IDiscordAuthContext authContext)
        {
            this._auth = authContext;
        }

        public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            string mode = _defaultMode;
            if (placeholder.Groups[1].Success)
                mode = placeholder.Groups[1].Value;

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
