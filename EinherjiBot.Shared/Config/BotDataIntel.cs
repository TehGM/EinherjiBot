using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TehGM.EinherjiBot.Extensions;
using TehGM.EinherjiBot.DataModels;
using System.Linq;

namespace TehGM.EinherjiBot.Config
{
    public class BotDataIntel
    {
        public const string DefaultPath = "Config/data_intel.json";
        public event Action<BotDataIntel> Saving;

        [JsonIgnore]
        public IDictionary<ulong, UserIntel> UserIntel { get; private set; }

        [JsonConstructor]
        public BotDataIntel(UserIntel[] intelCollection)
        {
            UserIntel = new Dictionary<ulong, UserIntel>(intelCollection == null ? 0 : intelCollection.Length);
            if (intelCollection == null)
                return;
            for (int i = 0; i < intelCollection.Length; i++)
            {
                UserIntel intel = intelCollection[i];
                UserIntel.Add(intel.UserID, intel);
            }
        }

        public UserIntel GetOrCreateUserIntel(ulong userId)
        {
            if (!UserIntel.TryGetValue(userId, out UserIntel result))
            {
                result = new UserIntel(userId);
                UserIntel.Add(userId, result);
            }
            return result;
        }

        public static async Task<BotDataIntel> LoadAsync(string filePath)
        {
            JToken fileContents = await JsonFileExtensions.LoadFromFileAsync(filePath);
            return fileContents.ToObject<BotDataIntel>();
        }

        public static Task<BotDataIntel> LoadAsync()
            => LoadAsync(DefaultPath);

        public Task SaveAsync(string filePath)
        {
            Saving?.Invoke(this);
            JObject obj = JObject.FromObject(this);
            obj.Add("intel", JToken.FromObject(UserIntel.Values));
            return JsonFileExtensions.SaveToFileAsync((JToken)obj, filePath);
        }

        public Task SaveAsync()
            => SaveAsync(DefaultPath);
    }
}
