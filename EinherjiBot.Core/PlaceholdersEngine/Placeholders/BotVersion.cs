namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("BotVersion", PlaceholderUsage.Any)]
    [DisplayName("Bot Version")]
    public class BotVersionPlaceholder
    {
        public class BotVersionPlaceholderHandler : PlaceholderHandler<BotVersionPlaceholder>
        {
            protected override Task<string> GetReplacementAsync(BotVersionPlaceholder placeholder, CancellationToken cancellationToken = default)
                => Task.FromResult(EinherjiInfo.BotVersion);
        }
    }
}
