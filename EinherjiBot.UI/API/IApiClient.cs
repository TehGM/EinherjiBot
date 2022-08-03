using System.Net.Http;

namespace TehGM.EinherjiBot.UI.API
{
    public interface IApiClient
    {
        Task<TResponse> SendJsonAsync<TResponse>(HttpRequestMessage request, object data, CancellationToken cancellationToken = default);
    }
}
