using Discord;
using Discord.Interactions;
using TehGM.EinherjiBot.Auditing;
using TehGM.EinherjiBot.Auditing.Intel;

namespace TehGM.EinherjiBot.Intel.Commands
{
    [Group("intel", "Get Discord intel reports")]
    [EnabledInDm(true)]
    public class UserIntelSlashCommands : EinherjiInteractionModule
    {
        [Group("on", "Get Discord intel reports")]
        public class UserIntelOnCommands : EinherjiInteractionModule
        {
            private readonly IIntelEmbedBuilder _embed;
            private readonly IAuditStore<UserIntelAuditEntry> _userAudit;
            private readonly IAuditStore<GuildIntelAuditEntry> _guildAudit;

            public UserIntelOnCommands(IIntelEmbedBuilder embedBuilder, IAuditStore<UserIntelAuditEntry> userAudit, IAuditStore<GuildIntelAuditEntry> guildAudit)
            {
                this._embed = embedBuilder;
                this._userAudit = userAudit;
                this._guildAudit = guildAudit;
            }

            [SlashCommand("user", "Gets intel on specific user")]
            public async Task CmdIntelUserAsync(
                [Summary("User", "User to get intel on")] IUser user)
            {
                Embed embed = await this._embed.BuildUserEmbedAsync(user, base.Context.Guild, base.CancellationToken).ConfigureAwait(false);
                await this._userAudit.AddAuditAsync(new UserIntelAuditEntry(base.Context.User.Id, user.Id, base.Context.Interaction.CreatedAt.UtcDateTime));
                await base.RespondAsync(null, embed).ConfigureAwait(false);
            }

            [SlashCommand("me", "Gets intel on you")]
            public Task CmdIntelMeAsync()
            {
                return this.CmdIntelUserAsync(base.Context.User);
            }

            [SlashCommand("guild", "Gets intel on current guild")]
            [EnabledInDm(false)]
            public async Task CmdIntelGuildAsync()
            {
                IGuild guild = base.Context.Guild;
                Embed embed = await this._embed.BuildGuildEmbedAsync(guild, base.CancellationToken).ConfigureAwait(false);
                await this._guildAudit.AddAuditAsync(new GuildIntelAuditEntry(base.Context.User.Id, base.Context.Guild.Id, base.Context.Interaction.CreatedAt.UtcDateTime));
                await base.RespondAsync(null, embed).ConfigureAwait(false);
            }
        }
    }
}
