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
            ISharedAccountHandler handler = services.GetRequiredService<ISharedAccountHandler>();

            SharedAccountFilter filter = new SharedAccountFilter();
            filter.AccountType = this.AccountType;
            filter.LoginContains = autocompleteInteraction.Data.Current.Value.ToString();
            if (this.ForModeration)
                filter.ModUserID = context.User.Id;

            IEnumerable<ISharedAccount> accounts = await handler.GetAllAsync(filter, skipAudit: true, (context as EinherjiInteractionContext).CancellationToken).ConfigureAwait(false);
            return AutocompletionResult.FromSuccess(accounts
                .Select(s => new AutocompleteResult(s.Login, s.ID.ToString()))
                .Take(25));
        }
    }

    public class NetflixSharedAccountAutocompleteHandler : SharedAccountAutocompleteHandlerBase
    {
        protected override SharedAccountType AccountType => SharedAccountType.Netflix;
        protected override bool ForModeration => false;
    }

    public class NetflixSharedAccountModerationAutocompleteHandler : SharedAccountAutocompleteHandlerBase
    {
        protected override SharedAccountType AccountType => SharedAccountType.Netflix;
        protected override bool ForModeration => true;
    }
}
