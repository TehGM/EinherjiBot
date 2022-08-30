using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("CurrentGuild", PlaceholderUsage.GuildMessageContext | PlaceholderUsage.GuildEvent)]
    [DisplayName("Current Guild")]
    [Description("Is replaced with name of guild the message was sent in.")]
    public class CurrentGuildPlaceholder
    {
        public class CurrentGuildPlaceholderHandler : PlaceholderHandler<CurrentGuildPlaceholder>
        {
            private readonly PlaceholderConvertContext _context;
            private readonly IDiscordEntityInfoProvider _provider;

            public CurrentGuildPlaceholderHandler(PlaceholderConvertContext context, IDiscordEntityInfoProvider provider)
            {
                this._context = context;
                this._provider = provider;
            }

            protected override async Task<string> GetReplacementAsync(CurrentGuildPlaceholder placeholder, CancellationToken cancellationToken = default)
            {
                if (this._context.CurrentGuildID == null)
                    throw new PlaceholderContextException($"{nameof(CurrentGuildPlaceholder)} can only be used for guild messages.");

                IDiscordGuildInfo guild = await this._provider.GetGuildInfoAsync(this._context.CurrentGuildID.Value).ConfigureAwait(false);
                return guild.Name;
            }
        }
    }
}
