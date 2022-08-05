using System.Net.Http;

namespace TehGM.EinherjiBot.UI.API
{
    public class ClientVersionException : HttpRequestException
    {
        public ClientVersionException(string expectedVersion)
            : base($"Current client version is {EinherjiInfo.WebVersion} while expected version is {expectedVersion}", null, System.Net.HttpStatusCode.BadRequest) { }
    }
}
