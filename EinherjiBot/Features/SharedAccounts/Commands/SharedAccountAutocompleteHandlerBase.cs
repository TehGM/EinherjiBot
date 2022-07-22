using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using TehGM.EinherjiBot.DiscordClient;

namespace TehGM.EinherjiBot.SharedAccounts.Commands
{
    public abstract class SharedAccountAutocompleteHandlerBase : AutocompleteHandler
    {
        protected abstract SharedAccountType AccountType { get; }
        protected abstract bool ForModeration { get; }

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        {
            ISharedAccountProvider provider = services.GetRequiredService<ISharedAccountProvider>();
            EinherjiInteractionContext ctx = (EinherjiInteractionContext)context;

            IEnumerable<SharedAccount> servers = await provider.GetOfTypeAsync(this.AccountType, this.ForModeration, ctx.CancellationToken).ConfigureAwait(false);

            string input = autocompleteInteraction.Data.Current.Value.ToString();
            if (!string.IsNullOrEmpty(input))
                servers = servers.Where(a => a.Login.StartsWith(input, StringComparison.InvariantCultureIgnoreCase));
            return AutocompletionResult.FromSuccess(servers
                .Select(s => new AutocompleteResult(s.Login, s.ID.ToString()))
                .Take(25));
        }
    }
}
