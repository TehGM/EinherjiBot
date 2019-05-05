using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.Config
{
    public class BotAuth : IDisposable
    {
        [JsonProperty("token", Required = Required.Always)]
        public string Token { get; private set; }

        public static async Task<BotAuth> LoadAsync(string filePath)
        {
            JToken fileContents = await JsonFileExtensions.LoadFromFileAsync(filePath);
            return fileContents.ToObject<BotAuth>();
        }

        public static Task<BotAuth> LoadAsync()
            => LoadAsync("Config/auth.json");

        public void Dispose()
        {
            this.Token = null;
        }
    }
}
