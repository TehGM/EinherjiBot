namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("{{CurrentUserMention}}")]
    public class CurrentUserMention : IPlaceholder
    {
        private readonly IDiscordAuthContext _auth;

        public CurrentUserMention(IDiscordAuthContext authContext)
        {
            this._auth = authContext;
        }

        public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
            => Task.FromResult(this._auth.DiscordUser.Mention);
    }
}
