using Discord.Interactions;
using Discord.WebSocket;

namespace TehGM.EinherjiBot.DiscordClient
{
    public class EinherjiInteractionContext : SocketInteractionContext<SocketInteraction>
    {
        public IDiscordAuthContext AuthContext { get; }
        public CancellationToken CancellationToken { get; }

        public EinherjiInteractionContext(DiscordSocketClient client, SocketInteraction interaction, IDiscordAuthContext authContext, CancellationToken cancellationToken)
             : base(client, interaction)
        {
            this.AuthContext = authContext;
            this.CancellationToken = cancellationToken;
        }
    }
}
