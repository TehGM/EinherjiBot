using Discord;
using Discord.Interactions;
using System.Text;
using TehGM.EinherjiBot.Auditing;
using TehGM.EinherjiBot.Auditing.GameServer;

namespace TehGM.EinherjiBot.GameServers.Commands
{
    [Group("game-server", "Retrieves info for our game servers that you have access to")]
    public class GameServerSlashCommands : EinherjiInteractionModule
    {
        private readonly IGameServerProvider _provider;
        private readonly IAuditStore<GameServerAuditEntry> _audit;

        public GameServerSlashCommands(IGameServerProvider provider)
        {
            this._provider = provider;
        }

        [SlashCommand("info", "Retrieves info for our game servers that you have access to")]
        public async Task CmdInfoAsync(
            [Summary("Server", "Name of the server to pick"), Autocomplete(typeof(GameServerAutocompleteHandler))] Guid id)
        {
            GameServer server = await this._provider.GetAsync(id, base.CancellationToken).ConfigureAwait(false);

            if (server == null)
            {
                await base.RespondAsync($"{EinherjiEmote.FailureSymbol} Requested game server not found.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder();
            embed.Title = $"{server.Name} Server Info";
            embed.Color = Color.Blue;
            StringBuilder builder = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(server.RulesURL))
                builder.AppendFormat("Before connecting, please read the [server rules]({0}).\n\n", server.RulesURL);
            builder.AppendFormat("***Address***: `{0}`\n", server.Address);
            if (!string.IsNullOrWhiteSpace(server.Password))
                builder.AppendFormat("***Password***: `{0}`\n", server.Password);
            if (!string.IsNullOrWhiteSpace(server.ImageURL))
                embed.WithThumbnailUrl(server.ImageURL);
            embed.Description = builder.ToString();

            await this._audit.AddAuditAsync(new GameServerAuditEntry(base.Context.User.Id, server.ID, base.Context.Interaction.CreatedAt.UtcDateTime));
            await base.RespondAsync(null, embed.Build()).ConfigureAwait(false);
        }
    }
}
