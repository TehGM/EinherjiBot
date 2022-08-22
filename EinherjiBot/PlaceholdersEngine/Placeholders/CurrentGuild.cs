using Discord;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    public class CurrentGuildPlaceholderHandler : PlaceholderHandler<CurrentGuildPlaceholder>
    {
        private readonly IMessage _message;

        public CurrentGuildPlaceholderHandler(IMessage message)
        {
            this._message = message;
        }

        protected override Task<string> GetReplacementAsync(CurrentGuildPlaceholder placeholder, CancellationToken cancellationToken = default)
        {
            if (this._message.Channel is not IGuildChannel channel)
                throw new PlaceholderConvertException($"{nameof(CurrentGuildPlaceholder)} can only be used for guild messages");
            return Task.FromResult(channel.Guild.Name);
        }
    }
}
