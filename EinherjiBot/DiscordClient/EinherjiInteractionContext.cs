using Discord.Interactions;
using Discord.WebSocket;
using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.DiscordClient
{
    public class EinherjiInteractionContext : SocketInteractionContext<SocketInteraction>
    {
        public IAuthContext AuthContext { get; }
        public CancellationToken CancellationToken { get; }

        public EinherjiInteractionContext(DiscordSocketClient client, SocketInteraction interaction, IAuthContext authContext, CancellationToken cancellationToken)
             : base(client, interaction)
        {
            this.AuthContext = authContext;
            this.CancellationToken = cancellationToken;
        }
    }
}
