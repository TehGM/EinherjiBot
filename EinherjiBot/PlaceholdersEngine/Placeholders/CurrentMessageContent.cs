using Discord;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    // this placeholder will break when not in message context (like message trigger)
    [OldPlaceholder("{{CurrentMessageContent}}")]
    public class CurrentMessageContent : IPlaceholder
    {
        private readonly IMessage _message;

        public CurrentMessageContent(IMessage message)
        {
            this._message = message;
        }

        public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
            => Task.FromResult(this._message.Content);
    }
}
