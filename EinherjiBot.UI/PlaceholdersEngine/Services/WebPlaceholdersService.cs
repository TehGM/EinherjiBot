using System.Text.RegularExpressions;
using TehGM.EinherjiBot.PlaceholdersEngine;
using TehGM.EinherjiBot.PlaceholdersEngine.API;
using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.UI.API;

namespace TehGM.EinherjiBot.UI.PlaceholdersEngine.Services
{
    public class WebPlaceholdersService : IPlaceholdersService
    {
        private static readonly Regex _placeholderCheckRegex = new Regex(@"{{.+}}", RegexOptions.Singleline);

        private readonly IApiClient _client;
        private readonly IPlaceholdersEngine _engine;
        private readonly IAuthProvider _auth;
        private readonly ILogger _log;

        public WebPlaceholdersService(IApiClient client, IPlaceholdersEngine engine, ILogger<WebPlaceholdersService> log, IAuthProvider auth)
        {
            this._client = client;
            this._engine = engine;
            this._log = log;
            this._auth = auth;
        }

        public async Task<PlaceholdersConvertResponse> ConvertAsync(PlaceholdersConvertRequest request, CancellationToken cancellationToken = default)
        {
            // there's no need to tax server with the request and running the engine if there's no placeholders present in request, so just short-circuit
            if (!_placeholderCheckRegex.IsMatch(request.Value))
                return new PlaceholdersConvertResponse(request.Value);

            // if possible, gonna run the engine client-side. This will allow use client's cache if possible without calling the API
            try
            {
                if (request.Context.CurrentUserID == null && this._auth.User.IsLoggedIn())
                    request.Context.CurrentUserID = this._auth.User.ID;

                string result = await this._engine.ConvertPlaceholdersAsync(request.Value, request.Context, cancellationToken);
                return new PlaceholdersConvertResponse(result);
            }
            catch (InvalidOperationException)
            {
                this._log.LogDebug("Client-side placeholders engine couldn't convert some placeholders, requesting server-side run");
                return await this._client.PostJsonAsync<PlaceholdersConvertResponse>("placeholders/convert", request, cancellationToken);
            }
        }
    }
}
