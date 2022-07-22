using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using TehGM.EinherjiBot.DiscordClient;

namespace TehGM.EinherjiBot.GameServers.Commands
{
    public class GameServerAutocompleteHandler : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            IGameServerProvider provider = services.GetRequiredService<IGameServerProvider>();
            EinherjiInteractionContext ctx = (EinherjiInteractionContext)context;

            IEnumerable<GameServer> servers = await provider.GetAllAsync(ctx.CancellationToken).ConfigureAwait(false);

            string input = autocompleteInteraction.Data.Current.Value.ToString();
            if (!string.IsNullOrEmpty(input))
                servers = servers.Where(s => s.Name.StartsWith(input, StringComparison.InvariantCultureIgnoreCase));
            return AutocompletionResult.FromSuccess(servers
                .Select(s => new AutocompleteResult(s.Name, s.ID.ToString()))
                .Take(25));
        }
    }
}
