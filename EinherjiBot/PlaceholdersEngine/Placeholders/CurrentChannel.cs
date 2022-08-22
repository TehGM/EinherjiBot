using Discord;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    internal class CurrentChannelPlaceholderHandler : PlaceholderHandler<CurrentChannelPlaceholder>
    {
        private readonly IMessage _message;

        public CurrentChannelPlaceholderHandler(IMessage message)
        {
            this._message = message;
        }

        protected override Task<string> GetReplacementAsync(CurrentChannelPlaceholder placeholder, CancellationToken cancellationToken = default)
        {
            IChannel channel = this._message.Channel;

            switch (placeholder.DisplayMode)
            {
                case ChannelDisplayMode.Mention:
                    return Task.FromResult(MentionUtils.MentionChannel(channel.Id));
                case ChannelDisplayMode.Name:
                    return Task.FromResult(channel.Name);
                default:
                    throw new ArgumentException($"Unsupported display mode {placeholder.DisplayMode}", nameof(placeholder.DisplayMode));
            }
        }
    }
}
