namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [OldPlaceholder("{{BotVersion}}")]
    public class BotVersion : IPlaceholder
    {
        public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
            => Task.FromResult(EinherjiInfo.BotVersion);
    }
}
