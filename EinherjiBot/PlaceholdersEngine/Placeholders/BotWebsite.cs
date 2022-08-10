namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [OldPlaceholder("{{BotWebsite}}")]
    public class BotWebsite : IPlaceholder
    {
        public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
            => Task.FromResult(EinherjiInfo.WebsiteURL);
    }
}
