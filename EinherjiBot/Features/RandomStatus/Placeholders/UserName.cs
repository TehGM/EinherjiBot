using Discord;

namespace TehGM.EinherjiBot.RandomStatus.Placeholders
{
    [StatusPlaceholder($"{{{{UserName:(\\d{{1,20}})}}}}")]
    internal class UserName : IStatusPlaceholder
    {
        private readonly IDiscordClient _client;

        private string _name;

        public UserName(IDiscordClient client)
        {
            this._client = client;
        }

        public async Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrEmpty(this._name))
                return this._name;

            if (!placeholder.Groups[1].Success)
                throw new ArgumentException($"Placeholder requires a valid user ID to be provided");
            if (!ulong.TryParse(placeholder.Groups[1].Value, out ulong id))
                throw new ArgumentException($"Placeholder: {placeholder.Groups[1].Value} is not a valid user ID");

            IUser user = await this._client.GetUserAsync(id, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (user == null)
                throw new InvalidOperationException($"Discord user with ID {id} not found");
            this._name = user.Username;
            return this._name;
        }
    }
}
