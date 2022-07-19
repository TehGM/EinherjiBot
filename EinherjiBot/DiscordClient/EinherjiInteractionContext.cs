using Discord.Interactions;
using Discord.WebSocket;
using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.DiscordClient
{
    public class EinherjiInteractionContext : SocketInteractionContext<SocketInteraction>
    {
        public IUserContext UserSecurityContext { get; }
        public CancellationToken CancellationToken { get; }

        public EinherjiInteractionContext(DiscordSocketClient client, SocketInteraction interaction, IUserContext userContext, CancellationToken cancellationToken)
             : base(client, interaction)
        {
            this.UserSecurityContext = userContext;
            this.CancellationToken = cancellationToken;
        }
    }
}
