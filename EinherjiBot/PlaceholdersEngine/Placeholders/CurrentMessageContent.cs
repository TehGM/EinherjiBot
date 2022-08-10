using Discord;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [OldPlaceholder("{{CurrentMessageContent}}")]
    public class CurrentMessageContentPlaceholderHandler : PlaceholderHandler<CurrentMessageContentPlaceholder>
    {
        private readonly IMessage _message;

        public CurrentMessageContentPlaceholderHandler(IMessage message)
        {
            this._message = message;
        }

        protected override Task<string> GetReplacementAsync(CurrentMessageContentPlaceholder placeholder, CancellationToken cancellationToken = default)
            => Task.FromResult(this._message.Content);
    }
}
