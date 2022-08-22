namespace TehGM.EinherjiBot.PlaceholdersEngine.API.Services
{
    public class ApiPlaceholdersService : IPlaceholdersService
    {
        private readonly IPlaceholdersEngine _engine;

        public ApiPlaceholdersService(IPlaceholdersEngine engine)
        {
            this._engine = engine;
        }

        public async Task<PlaceholdersConvertResponse> ConvertAsync(PlaceholdersConvertRequest request, CancellationToken cancellationToken = default)
        {
            string result = await this._engine.ConvertPlaceholdersAsync(request.Value, request.Context, cancellationToken).ConfigureAwait(false);
            return new PlaceholdersConvertResponse(result);
        }
    }
}
