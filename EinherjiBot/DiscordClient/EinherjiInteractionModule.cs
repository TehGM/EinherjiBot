using Discord;
using Discord.Interactions;
using TehGM.EinherjiBot.DiscordClient;

namespace TehGM.EinherjiBot
{
    public class EinherjiInteractionModule<TContext> : InteractionModuleBase<TContext> where TContext : class, IInteractionContext
    {
        protected Task RespondAsync(string text, Embed embed, CancellationToken cancellationToken)
            => base.RespondAsync(text: text, embed: embed, options: this.GetRequestOptions(cancellationToken));
        protected Task RespondAsync(string text, CancellationToken cancellationToken)
            => this.RespondAsync(text, null, cancellationToken);
        protected Task RespondAsync(Embed embed, CancellationToken cancellationToken)
            => this.RespondAsync(null, embed, cancellationToken);

        protected RequestOptions GetRequestOptions(CancellationToken cancellationToken)
            => cancellationToken.ToRequestOptions();
    }

    public class EinherjiInteractionModule : EinherjiInteractionModule<EinherjiInteractionContext>
    {
        public Task RespondAsync(string text, Embed embed)
            => base.RespondAsync(text, embed, base.Context.CancellationToken);
        public Task RespondAsync(string text)
            => base.RespondAsync(text, base.Context.CancellationToken);
        public Task RespondAsync(Embed embed)
            => base.RespondAsync(embed, base.Context.CancellationToken);

        protected RequestOptions GetRequestOptions()
            => this.GetRequestOptions(base.Context.CancellationToken);
    }
}
