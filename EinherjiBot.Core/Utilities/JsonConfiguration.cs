using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace TehGM.EinherjiBot.Utilities
{
    public static class JsonConfiguration
    {
        public static void InitializeDefaultSettings()
        {
            JsonConvert.DefaultSettings = GetDefaultSettings;
        }

        public static JsonSerializerSettings GetDefaultSettings()
        {
            JsonSerializerSettings result = new JsonSerializerSettings();
            ApplyDefaultSettings(result);
            return result;
        }

        public static void ApplyDefaultSettings(JsonSerializerSettings settings)
        {
            settings.Converters.Add(new UnixTimestampConverter());
            settings.Converters.Add(new StringEnumConverter());
        }
    }
}
