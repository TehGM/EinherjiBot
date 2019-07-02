using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using TehGM.EinherjiBot.Extensions;
using System.Threading.Tasks;
using TehGM.EinherjiBot.DataModels;
using System;
using System.ComponentModel;

namespace TehGM.EinherjiBot.Config
{
    public class BotConfig
    {
        public const string DefaultPath = "Config/config.json";

        [JsonIgnore]
        public BotAuth Auth { get; private set; }
        [JsonIgnore]
        public BotData Data { get; private set; }

        [JsonProperty("authorId")]
        public ulong AuthorID { get; private set; }
        [JsonProperty("botChannels")]
        public BotChannelsInfo BotChannels { get; private set; }
        [JsonProperty("eliteApi")]
        public EliteApiConfig EliteAPI { get; private set; }
        [JsonProperty("defaultConfirmString", NullValueHandling = NullValueHandling.Include, DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("\u274C")]
        public string DefaultConfirm { get; private set; }
        [JsonProperty("defaultRejectString", NullValueHandling = NullValueHandling.Include, DefaultValueHandling = DefaultValueHandling.Populate)]
        [DefaultValue("\u2705")]
        public string DefaultReject { get; private set; }

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
            => LoadAsync(DefaultPath);

        public Task SaveAllAsync()
            => Data?.SaveAsync();
    }
}
