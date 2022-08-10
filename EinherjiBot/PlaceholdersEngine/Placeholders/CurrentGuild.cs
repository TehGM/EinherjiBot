using Discord;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    // this placeholder will break when not in message context (like message trigger)
    [OldPlaceholder("{{CurrentGuild}}")]
    public class CurrentGuild : IPlaceholder
    {
        private readonly IMessage _message;

        public CurrentGuild(IMessage message)
        {
            this._message = message;
        }

        public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            if (this._message.Channel is not IGuildChannel channel)
                throw new InvalidOperationException($"{nameof(CurrentGuild)} placeholder can only be used for guild messages");
            return Task.FromResult(channel.Guild.Name);
        }
    }
}
