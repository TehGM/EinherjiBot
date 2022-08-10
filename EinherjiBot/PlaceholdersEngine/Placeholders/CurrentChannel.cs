using Discord;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    // this placeholder will break when not in message context (like message trigger)
    [OldPlaceholder($"{{{{CurrentChannel(?::({_modeMention}|{_modeName}))?}}}}", DisplayName = "{{CurrentChannel}}")]
    public class CurrentChannel : IPlaceholder
    {
        private const string _modeMention = "Mention";
        private const string _modeName = "Name";
        private const string _defaultMode = _modeMention;

        private readonly IMessage _message;

        public CurrentChannel(IMessage message)
        {
            this._message = message;
        }

        public Task<string> GetReplacementAsync(Match placeholder, CancellationToken cancellationToken = default)
        {
            string mode = _defaultMode;
            if (placeholder.Groups[1].Success)
                mode = placeholder.Groups[1].Value;

            if (mode == _modeMention)
                return Task.FromResult(MentionUtils.MentionChannel(this._message.Channel.Id));
            else
                return Task.FromResult(this._message.Channel.Name);
        }
    }
}
