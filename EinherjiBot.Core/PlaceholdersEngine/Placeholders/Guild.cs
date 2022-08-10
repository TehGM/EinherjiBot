using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("Guild", PlaceholderUsage.Any)]
    public class GuildPlaceholder
    {
        [PlaceholderProperty("ID", IsRequired = true, IDType = IDType.Guild)]
        public ulong GuildID { get; init; }

        public class GuildPlaceholderHandler : PlaceholderHandler<GuildPlaceholder>
        {
            private readonly IDiscordEntityInfoService _provider;

            public GuildPlaceholderHandler(IDiscordEntityInfoService provider)
            {
                this._provider = provider;
            }

            protected async override Task<string> GetReplacementAsync(GuildPlaceholder placeholder, CancellationToken cancellationToken = default)
            {
                IDiscordGuildInfo guild = await this._provider.GetGuildInfoAsync(placeholder.GuildID, cancellationToken).ConfigureAwait(false);
                if (guild == null)
                    throw new InvalidOperationException($"Discord guild with ID {placeholder.GuildID} not found");

                return guild.Name;
            }
        }
    }
}
