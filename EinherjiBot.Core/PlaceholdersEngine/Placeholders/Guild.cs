using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("Guild", PlaceholderUsage.Admin)]
    public class GuildPlaceholder
    {
        [PlaceholderProperty("ID", IsRequired = true, IDType = IDType.Guild)]
        public ulong GuildID { get; init; }

        public class GuildPlaceholderHandler : PlaceholderHandler<GuildPlaceholder>
        {
            private readonly IDiscordEntityInfoService _provider;
            private readonly IAuthContext _auth;

            public GuildPlaceholderHandler(IDiscordEntityInfoService provider, IAuthContext auth)
            {
                this._provider = provider;
                this._auth = auth;
            }

            protected async override Task<string> GetReplacementAsync(GuildPlaceholder placeholder, CancellationToken cancellationToken = default)
            {
                if (!this._auth.IsAdmin() && !this._auth.IsEinherji())
                    throw new AccessForbiddenException("You're not authorized to use one or more placeholders");

                IDiscordGuildInfo guild = await this._provider.GetGuildInfoAsync(placeholder.GuildID, cancellationToken).ConfigureAwait(false);
                if (guild == null)
                    throw new InvalidOperationException($"Discord guild with ID {placeholder.GuildID} not found");

                return guild.Name;
            }
        }
    }
}
