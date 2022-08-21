using System.Text.RegularExpressions;
using TehGM.EinherjiBot.PlaceholdersEngine.API;
using TehGM.EinherjiBot.UI.API;

namespace TehGM.EinherjiBot.UI.PlaceholdersEngine.Services
{
    public class WebPlaceholdersService : IPlaceholdersService
    {
        private static readonly Regex _placeholderCheckRegex = new Regex(@"{{.+}}", RegexOptions.Singleline);

        private readonly IApiClient _client;

        public WebPlaceholdersService(IApiClient client)
        {
            this._client = client;
        }

        public Task<PlaceholdersConvertResponse> ConvertAsync(PlaceholdersConvertRequest request, CancellationToken cancellationToken = default)
        {
            // there's no need to tax server with the request and running the engine if there's no placeholders present in request, so just short-circuit
            if (!_placeholderCheckRegex.IsMatch(request.Value))
                return Task.FromResult(new PlaceholdersConvertResponse(request.Value));

            return this._client.PostJsonAsync<PlaceholdersConvertResponse>("placeholders/convert", request, cancellationToken);
        }
    }
}
