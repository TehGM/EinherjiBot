using System.Net.Http;
using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.UI.API
{
    public class ClientVersionException : ApiException
    {
        public ClientVersionException(string expectedVersion)
            : base($"Current client version is {EinherjiInfo.WebVersion} while expected version is {expectedVersion}", null, System.Net.HttpStatusCode.BadRequest) { }
    }
}
