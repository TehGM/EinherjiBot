namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("BotWebsite", PlaceholderUsage.Any)]
    public class BotWebsitePlaceholder
    {
        public class BotWebsitePlaceholderHandler : PlaceholderHandler<BotWebsitePlaceholder>
        {
            protected override Task<string> GetReplacementAsync(BotWebsitePlaceholder placeholder, CancellationToken cancellationToken = default)
                => Task.FromResult(EinherjiInfo.WebsiteURL);
        }
    }
}
