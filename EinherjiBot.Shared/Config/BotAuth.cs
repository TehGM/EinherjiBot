using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
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
}
