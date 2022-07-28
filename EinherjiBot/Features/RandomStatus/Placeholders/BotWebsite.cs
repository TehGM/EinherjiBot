namespace TehGM.EinherjiBot.RandomStatus.Placeholders
{
    [StatusPlaceholder("{{BotWebsite}}")]
    public class BotWebsite : IStatusPlaceholder
    {
        public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
            => Task.FromResult(EinherjiInfo.WebsiteURL);
    }
}
