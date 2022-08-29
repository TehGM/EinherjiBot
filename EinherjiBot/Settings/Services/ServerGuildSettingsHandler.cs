using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Auditing;
using TehGM.EinherjiBot.Settings.Policies;
using TehGM.EinherjiBot.Auditing.Settings;
using Discord;
using TehGM.EinherjiBot.DiscordClient;

namespace TehGM.EinherjiBot.Settings.Services
{
    public class ServerGuildSettingsHandler : IGuildSettingsHandler
    {
        private readonly IGuildSettingsProvider _provider;
        private readonly IDiscordClient _client;
        private readonly IDiscordConnection _connection;
        private readonly IAuthContext _user;
        private readonly IBotAuthorizationService _auth;
        private readonly IAuditStore<GuildSettingsAuditEntry> _audit;

        public ServerGuildSettingsHandler(IGuildSettingsProvider provider, IDiscordClient client, IDiscordConnection connection,
            IAuthContext user, IBotAuthorizationService auth, IAuditStore<GuildSettingsAuditEntry> audit)
        {
            this._provider = provider;
            this._client = client;
            this._connection = connection;
            this._user = user;
            this._auth = auth;
            this._audit = audit;
        }

        public async Task<GuildSettingsResponse> GetAsync(ulong guildID, CancellationToken cancellationToken = default)
        {
            IGuildSettings settings = await this.GetInternalAsync(guildID, cancellationToken).ConfigureAwait(false);
            if (settings == null)
                return null;

            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(settings, typeof(CanManageGuildSettings), cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException(authorization.Reason);

            return this.CreateResponse(settings);
        }

        private async Task<GuildSettings> GetInternalAsync(ulong guildID, CancellationToken cancellationToken = default)
        {
            GuildSettings settings = await this._provider.GetAsync(guildID, cancellationToken).ConfigureAwait(false);
            if (settings == null)
            {
                await this._connection.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
                IGuild guild = await this._client.GetGuildAsync(guildID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
                if (guild == null)
                    return null;
                settings = GuildSettings.CreateDefault(guild);
            }
            return settings;
        }

        public async Task<EntityUpdateResult<GuildSettingsResponse>> UpdateAsync(ulong guildID, GuildSettingsRequest request, CancellationToken cancellationToken = default)
        {
            GuildSettings settings = await this.GetInternalAsync(guildID, cancellationToken).ConfigureAwait(false);
            if (settings == null)
                return null;

            IEnumerable<Type> policies = request.MaxMessageTriggers == settings.MaxMessageTriggers
                ? new[] { typeof(CanManageGuildSettings) }
                : new[] { typeof(CanManageGuildSettings), typeof(CanChangeGuildLimits) };
            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync<IGuildSettings>(settings, policies, cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException(authorization.Reason);

            if (settings.HasChanges(request))
            {
                this.ApplyChanges(settings, request);
                await this._provider.AddOrUpdateAsync(settings, cancellationToken).ConfigureAwait(false);
                await this._audit.AddAuditAsync(GuildSettingsAuditEntry.Updated(this._user.ID, settings.GuildID, DateTime.UtcNow), cancellationToken).ConfigureAwait(false);
                return IEntityUpdateResult.Saved(this.CreateResponse(settings));
            }
            else
                return IEntityUpdateResult.NoChanges(this.CreateResponse(settings));
        }

        private void ApplyChanges(GuildSettings settings, GuildSettingsRequest request)
        {
            settings.JoinNotificationChannelID = request.JoinNotificationChannelID;
            settings.LeaveNotificationChannelID = request.LeaveNotificationChannelID;
            settings.MaxMessageTriggers = request.MaxMessageTriggers;
        }

        private GuildSettingsResponse CreateResponse(IGuildSettings settings)
        {
            return new GuildSettingsResponse(settings.GuildID, settings.JoinNotificationChannelID, settings.LeaveNotificationChannelID, settings.MaxMessageTriggers);
        }
    }
}
