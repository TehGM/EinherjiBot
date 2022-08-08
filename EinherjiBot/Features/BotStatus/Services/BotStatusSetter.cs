using Discord;
using Discord.WebSocket;
using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.PlaceholdersEngine;
using TehGM.EinherjiBot.Security.Policies;
using TehGM.Utilities.Randomization;

namespace TehGM.EinherjiBot.BotStatus.Services
{
    public class BotStatusSetter : IBotStatusSetter
    {
        private readonly DiscordSocketClient _client;
        private readonly IRandomizer _randomizer;
        private readonly IStatusProvider _provider;
        private readonly IPlaceholdersEngine _placeholders;
        private readonly IBotAuthorizationService _auth;
        private readonly ILogger _log;

        public BotStatusSetter(DiscordSocketClient client, IRandomizer randomizer, IStatusProvider provider, IPlaceholdersEngine placeholders, 
            IBotAuthorizationService auth, ILogger<BotStatusSetter> log)
        {
            this._client = client;
            this._randomizer = randomizer;
            this._provider = provider;
            this._placeholders = placeholders;
            this._auth = auth;
            this._log = log;
        }

        public async Task<Status> RandomizeStatusAsync(CancellationToken cancellationToken = default)
        {
            if (this._client.CurrentUser == null || this._client.ConnectionState != ConnectionState.Connected)
                return null;

            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(typeof(AuthorizeBotOrAdmin), cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException("You are not authorized to change bot's status");

            IEnumerable<Status> statuses = await this._provider.GetAllAsync(cancellationToken).ConfigureAwait(false);
            statuses = statuses.Where(s => s.IsEnabled);
            if (!statuses.Any())
                return null;

            Status status = this._randomizer.GetRandomValue(statuses);
            if (status == null)
                return null;

            try
            {
                await this.SetStatusAsync(status, cancellationToken).ConfigureAwait(false);
                return status;
            }
            catch (Exception ex) when (ex.LogAsError(this._log, "Failed changing status to {Status}", status))
            {
                return null;
            }
        }

        public async Task SetStatusAsync(Status status, CancellationToken cancellationToken = default)
        {
            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(typeof(AuthorizeBotOrAdmin), cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException("You are not authorized to change bot's status");

            string text = status.Text;
            if (!string.IsNullOrWhiteSpace(status.Text))
            {
                text = await this._placeholders.ConvertPlaceholdersAsync(status.Text, cancellationToken).ConfigureAwait(false);
                this._log.LogDebug("Changing status to `{Status}`", text);
            }
            else
                this._log.LogDebug("Clearing status");
            await this._client.SetGameAsync(text, status.Link, status.ActivityType).ConfigureAwait(false);
        }
    }
}
