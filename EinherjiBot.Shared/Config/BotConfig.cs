using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.Config
{
    public class BotConfig
    {
        [JsonIgnore]
        public BotAuth Auth { get; private set; }
        [JsonIgnore]
        public BotData Data { get; private set; }

        [JsonProperty("authorId")]
        public ulong AuthorID { get; private set; }

        public static async Task<BotConfig> LoadAllAsync()
        {
            BotConfig config = await LoadAsync();
            try
            {
                config.Data = await BotData.LoadAsync();
            }
            catch (IOException) { }
            try
            {
                config.Auth = await BotAuth.LoadAsync();
            }
            catch (IOException) { }
            return config;
        }

        public static async Task<BotConfig> LoadAsync(string filePath)
        {
            JToken fileContents = await JsonFileExtensions.LoadFromFileAsync(filePath);
            return fileContents.ToObject<BotConfig>();
        }

        public static Task<BotConfig> LoadAsync()
            => LoadAsync("Config/config.json");
    }
}
