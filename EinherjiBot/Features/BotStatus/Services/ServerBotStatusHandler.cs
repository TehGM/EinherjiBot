using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Security.Policies;

namespace TehGM.EinherjiBot.BotStatus.Services
{
    public class ServerBotStatusHandler : IBotStatusHandler
    {
        private readonly IBotStatusProvider _provider;
        private readonly IBotStatusSetter _service;
        private readonly IBotAuthorizationService _auth;

        public ServerBotStatusHandler(IBotStatusProvider provider, IBotStatusSetter service, IBotAuthorizationService auth)
        {
            this._provider = provider;
            this._service = service;
            this._auth = auth;
        }

        public async Task<BotStatusResponse> CreateAsync(BotStatusRequest request, CancellationToken cancellationToken = default)
        {
            request.ThrowValidateForCreation();

            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(typeof(AuthorizeAdmin), cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException("You have no privileges to control bot's status.");

            BotStatus result = new BotStatus(request.Text, request.Link, request.ActivityType);
            result.IsEnabled = request.IsEnabled;

            await this._provider.AddOrUpdateAsync(result, cancellationToken).ConfigureAwait(false);
            return CreateResponse(result);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(typeof(AuthorizeAdmin), cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException("You have no privileges to control bot's status.");

            await this._provider.DeleteAsync(id, cancellationToken);
        }

        public async Task<IEnumerable<BotStatusResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(typeof(AuthorizeBotOrAdmin), cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException("You have no privileges to control bot's status.");

            IEnumerable<BotStatus> results = await this._provider.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return results.Select(CreateResponse);
        }

        public async Task<BotStatusResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(typeof(AuthorizeBotOrAdmin), cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException("You have no privileges to control bot's status.");

            BotStatus result = await this._provider.GetAsync(id, cancellationToken).ConfigureAwait(false);
            return result != null ? CreateResponse(result) : null;
        }

        public async Task<EntityUpdateResult<BotStatusResponse>> UpdateAsync(Guid id, BotStatusRequest request, CancellationToken cancellationToken = default)
        {
            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(typeof(AuthorizeAdmin), cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException("You have no privileges to control bot's status.");

            BotStatus status = await this._provider.GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (status == null)
                return null;

            request.ThrowValidateForUpdate(status);

            if (status.HasChanges(request))
            {
                status.Text = request.Text;
                status.Link = request.Link;
                status.ActivityType = request.ActivityType;
                status.IsEnabled = request.IsEnabled;
                status.LastError = null;
                await this._provider.AddOrUpdateAsync(status, cancellationToken).ConfigureAwait(false);
                return IEntityUpdateResult.Saved(CreateResponse(status));
            }
            else
                return IEntityUpdateResult.NoChanges(CreateResponse(status));
        }

        public async Task SetCurrentAsync(BotStatusRequest request, CancellationToken cancellationToken = default)
        {
            BotAuthorizationResult authorization = await this._auth.AuthorizeAsync(typeof(AuthorizeBotOrAdmin), cancellationToken).ConfigureAwait(false);
            if (!authorization.Succeeded)
                throw new AccessForbiddenException("You have no privileges to control bot's status.");

            BotStatus status = new BotStatus(request.Text, request.Link, request.ActivityType);
            await this._service.SetStatusAsync(status, cancellationToken);
        }

        private static BotStatusResponse CreateResponse(BotStatus status)
        {
            return new BotStatusResponse(status.ID, status.Text, status.Link, status.ActivityType, status.IsEnabled)
            {
                LastError = (ErrorInfoResponse)status.LastError
            };
        }
    }
}
