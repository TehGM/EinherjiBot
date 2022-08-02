using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TehGM.EinherjiBot.Utilities
{
    public static class JsonConfiguration
    {
        public static void InitializeDefaultSettings()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                Converters = new List<JsonConverter>()
                {
                    new UnixTimestampConverter(),
                    new StringEnumConverter()
                }
            };
        }
    }
}
