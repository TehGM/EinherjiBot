using System.Net.Http;

namespace TehGM.EinherjiBot.UI.API
{
    public interface IApiClient
    {
        HttpClient Client { get; }
    }
}
