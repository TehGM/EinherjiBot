using Discord;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder($"{{{{User:(\\d{{1,20}})}}}}(?::({_modeMention}|{_modeUsername}|{_modeUsernameWithDiscriminator}))?")]
    internal class User : IPlaceholder
    {
        private const string _modeMention = "Mention";
        private const string _modeUsername = "Username";
        private const string _modeUsernameWithDiscriminator = "UsernameWithDiscriminator";
        private const string _defaultMode = _modeUsername;

        private readonly IDiscordClient _client;

        public User(IDiscordClient client)
        {
            this._client = client;
        }

        public async Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            if (!placeholder.Groups[1].Success)
                throw new ArgumentException($"Placeholder requires a valid user ID to be provided");
            if (!ulong.TryParse(placeholder.Groups[1].Value, out ulong id))
                throw new ArgumentException($"Placeholder: {placeholder.Groups[1].Value} is not a valid user ID");

            IUser user = await this._client.GetUserAsync(id, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (user == null)
                throw new InvalidOperationException($"Discord user with ID {id} not found");

            string mode = _defaultMode;
            if (placeholder.Groups[1].Success)
                mode = placeholder.Groups[1].Value;

            if (mode == _modeMention)
                return user.Mention;

            if (mode == _modeUsername)
                return user.Username;
            else
                return user.GetUsernameWithDiscriminator();
        }
    }
}
