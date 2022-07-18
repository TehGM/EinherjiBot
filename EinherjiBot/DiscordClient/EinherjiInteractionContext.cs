using Discord.Interactions;
using Discord.WebSocket;

namespace TehGM.EinherjiBot.DiscordClient
{
    public class EinherjiInteractionContext : SocketInteractionContext<SocketInteraction>
    {
        public CancellationToken CancellationToken { get; }
        public EinherjiInteractionContext(DiscordSocketClient client, SocketInteraction interaction, CancellationToken cancellationToken)
             : base(client, interaction)
        {
            this.CancellationToken = cancellationToken;
        }
    }
}
