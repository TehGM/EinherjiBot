using System;
using System.Threading;
using System.Threading.Tasks;
using Discord;

namespace TehGM.EinherjiBot
{
    public static class UserExtensions
    {
        public static Task ModifyAsync(this IGuildUser user, Action<GuildUserProperties> func, CancellationToken cancellationToken)
            => user.ModifyAsync(func, new RequestOptions { CancelToken = cancellationToken });
    }
}
