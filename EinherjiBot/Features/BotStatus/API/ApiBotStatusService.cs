namespace TehGM.EinherjiBot.BotStatus.API.Services
{
    public class ApiBotStatusService : IBotStatusService
    {
        private readonly IStatusProvider _provider;
        private readonly IStatusService _service;

        public ApiBotStatusService(IStatusProvider provider, IStatusService service)
        {
            this._provider = provider;
            this._service = service;
        }

        public async Task<BotStatusResponse> CreateAsync(BotStatusRequest request, CancellationToken cancellationToken = default)
        {
            Status result = new Status(request.Text, request.Link, request.ActivityType);
            result.IsEnabled = request.IsEnabled;

            await this._provider.AddOrUpdateAsync(result, cancellationToken).ConfigureAwait(false);

            return FromStatus(result);
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
            => this._provider.DeleteAsync(id, cancellationToken);

        public async Task<IEnumerable<BotStatusResponse>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            IEnumerable<Status> results = await this._provider.GetAllAsync(cancellationToken).ConfigureAwait(false);
            return results.Select(FromStatus);
        }

        public async Task<BotStatusResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            Status result = await this._provider.GetAsync(id, cancellationToken).ConfigureAwait(false);
            return result != null ? FromStatus(result) : null;
        }

        public async Task<BotStatusResponse> UpdateAsync(Guid id, BotStatusRequest request, CancellationToken cancellationToken = default)
        {
            Status result = await this._provider.GetAsync(id, cancellationToken).ConfigureAwait(false);
            if (result == null)
                return null;

            result.Text = request.Text;
            result.Link = request.Link;
            result.ActivityType = request.ActivityType;
            result.IsEnabled = request.IsEnabled;
            await this._provider.AddOrUpdateAsync(result, cancellationToken).ConfigureAwait(false);

            return FromStatus(result);
        }

        private static BotStatusResponse FromStatus(Status status)
            => new BotStatusResponse(status.ID, status.Text, status.Link, status.ActivityType, status.IsEnabled);

        public Task SetCurrentAsync(BotStatusRequest request, CancellationToken cancellationToken = default)
        {
            Status status = new Status(request.Text, request.Link, request.ActivityType);
            return this._service.SetStatusAsync(status, cancellationToken);
        }
    }
}
