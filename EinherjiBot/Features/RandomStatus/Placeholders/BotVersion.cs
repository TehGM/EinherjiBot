namespace TehGM.EinherjiBot.RandomStatus.Placeholders
{
    [StatusPlaceholder("{{BotVersion}}")]
    public class BotVersion : IStatusPlaceholder
    {
        public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
            => Task.FromResult(EinherjiInfo.BotVersion);
    }
}
