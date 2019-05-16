using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TehGM.EinherjiBot.DataModels;
using TehGM.EinherjiBot.Extensions;

namespace TehGM.EinherjiBot.Config
{
    public class BotData
    {
        public const string DefaultPath = "Config/data.json";

        [JsonProperty("stellarisMods")]
        public List<StellarisModInfo> StellarisMods { get; set; }
        [JsonProperty("netflixAccount")]
        public NetflixAccountInfo NetflixAccount { get; set; }
        [JsonIgnore]
        public BotDataIntel Intel { get; set; }

        public static async Task<BotData> LoadAsync(string filePath)
        {
            JToken fileContents = await JsonFileExtensions.LoadFromFileAsync(filePath);
            BotData data = fileContents.ToObject<BotData>();
            data.Intel = await BotDataIntel.LoadAsync();
            return data;
        }

        public static Task<BotData> LoadAsync()
            => LoadAsync(DefaultPath);

        public Task SaveAsync(string filePath)
        {
            Task t1 = JsonFileExtensions.SaveToFileAsync(this, filePath);
            Task t2 = Intel.SaveAsync();
            return Task.WhenAll(t1, t2);
        }

        public Task SaveAsync()
            => SaveAsync(DefaultPath);
    }
}
