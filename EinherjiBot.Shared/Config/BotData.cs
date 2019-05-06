using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using TehGM.EinherjiBot.Extensions;

namespace TehGM.EinherjiBot.Config
{
    public class BotData
    {
        public const string DefaultPath = "Config/data.json";
        public static async Task<BotData> LoadAsync(string filePath)
        {
            JToken fileContents = await JsonFileExtensions.LoadFromFileAsync(filePath);
            return fileContents.ToObject<BotData>();
        }

        public static Task<BotData> LoadAsync()
            => LoadAsync(DefaultPath);

        public Task SaveAsync(string filePath)
            => JsonFileExtensions.SaveToFileAsync(this, filePath);

        public Task SaveAsync()
            => SaveAsync(DefaultPath);
    }
}
