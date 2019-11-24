using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog.Sinks.Datadog.Logs;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using TehGM.EinherjiBot.Extensions;
using TehGM.EinherjiBot.Utilities;

namespace TehGM.EinherjiBot.Config
{
    public class BotAuth : IDisposable
    {
        public const string DefaultPath = "Config/auth.json";

        [JsonProperty("token", Required = Required.Always)]
        public string Token { get; private set; }
        [JsonProperty("inaraApi")]
        public InaraApiAuth InaraAPI { get; private set; }
        [JsonProperty("datadogApi")]
        public DatadogApiAuth DatadogAPI { get; private set; }

        public static async Task<BotAuth> LoadAsync(string filePath)
        {
            Logging.Default.Debug("Loading bot auth config from {FilePath}", filePath);
            JToken fileContents = await JsonFileExtensions.LoadFromFileAsync(filePath);
            return fileContents.ToObject<BotAuth>();
        }

        public static Task<BotAuth> LoadAsync()
            => LoadAsync(DefaultPath);

        public void Dispose()
        {
            this.Token = null;
        }
    }

    public class DatadogApiAuth
    {
        [JsonProperty("apiKey")]
        public string ApiKey { get; private set; }
        [JsonProperty("url", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("intake.logs.datadoghq.com")]
        public string URL { get; private set; }
        [JsonProperty("port", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(10516)]
        public int Port { get; private set; }
        [JsonProperty("ssl", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(true)]
        public bool UseSSL { get; private set; }
        [JsonProperty("tcp", DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue(true)]
        public bool UseTCP { get; private set; }

        [JsonProperty("source")]
        public string Source { get; private set; }
        [JsonProperty("service")]
        public string Service { get; private set; }
        [JsonProperty("host")]
        public string Host { get; private set; }
        [JsonProperty("tags")]
        public string[] Tags { get; private set; }

        public DatadogConfiguration ToDatadogConfiguration()
            => new DatadogConfiguration(URL, Port, UseSSL, UseTCP);
    }
}
