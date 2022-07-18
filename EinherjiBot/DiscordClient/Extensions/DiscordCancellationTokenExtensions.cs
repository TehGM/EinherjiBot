using Discord;

namespace TehGM.EinherjiBot
{
    public static class DiscordCancellationTokenExtensions
    {
        public static RequestOptions ToRequestOptions(this CancellationToken cancellationToken, Action<RequestOptions> configure)
        {
            RequestOptions result = ToRequestOptions(cancellationToken);
            configure(result);
            return result;
        }

        public static RequestOptions ToRequestOptions(this CancellationToken cancellationToken)
            => new RequestOptions() { CancelToken = cancellationToken };
    }
}
