using System.Net;

namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    [Serializable]
    public class PlaceholderContextException : PlaceholderException
    {
        public PlaceholderContextException(PlaceholderDescriptor placeholder, Exception innerException = null)
            : this($"Placeholder `{placeholder.DisplayName}` is not usable in this context.", innerException) { }

        public PlaceholderContextException(string message, Exception innerException = null)
            : base(message, innerException, HttpStatusCode.BadRequest) { }
    }
}
