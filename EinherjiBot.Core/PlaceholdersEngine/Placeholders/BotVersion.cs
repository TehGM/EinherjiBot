namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("BotVersion", PlaceholderUsage.Any)]
    [DisplayName("Bot Version")]
    [Description("Is replaced with bot's current version.")]
    public class BotVersionPlaceholder
    {
        public class BotVersionPlaceholderHandler : PlaceholderHandler<BotVersionPlaceholder>
        {
            protected override Task<string> GetReplacementAsync(BotVersionPlaceholder placeholder, CancellationToken cancellationToken = default)
                => Task.FromResult(EinherjiInfo.BotVersion);
        }
    }
}
