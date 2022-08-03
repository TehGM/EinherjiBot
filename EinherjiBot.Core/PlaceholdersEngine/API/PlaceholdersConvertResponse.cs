using Newtonsoft.Json;

namespace TehGM.EinherjiBot.PlaceholdersEngine.API
{
    public class PlaceholdersConvertResponse
    {
        [JsonProperty("result", Required = Required.Always)]
        public string Result { get; }

        public PlaceholdersConvertResponse(string result)
        {
            if (result == null)
                throw new ArgumentNullException(nameof(result));
            this.Result = result;
        }
    }
}
