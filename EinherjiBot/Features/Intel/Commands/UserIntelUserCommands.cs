using Discord;
using Discord.Interactions;
using TehGM.EinherjiBot.Auditing;
using TehGM.EinherjiBot.Auditing.Intel;

namespace TehGM.EinherjiBot.Intel.Commands
{
    public class UserIntelUserCommands : EinherjiInteractionModule
    {
        private readonly IIntelEmbedBuilder _embed;
        private readonly IAuditStore<UserIntelAuditEntry> _audit;

        public UserIntelUserCommands(IIntelEmbedBuilder embedBuilder, IAuditStore<UserIntelAuditEntry> audit)
        {
            this._embed = embedBuilder;
            this._audit = audit;
        }

        [UserCommand("Get Intel")]
        public async Task CmdIntelUserAsync(IUser user)
        {
            Embed embed = await this._embed.BuildUserEmbedAsync(user, base.Context.Guild, base.CancellationToken).ConfigureAwait(false);
            await this._audit.AddAuditAsync(new UserIntelAuditEntry(base.Context.User.Id, user.Id, base.Context.Interaction.CreatedAt.UtcDateTime));
            await base.RespondAsync(null, embed).ConfigureAwait(false);
        }
    }
}
