using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TehGM.EinherjiBot.DataMigration
{
    public static class JsonFileUtility
    {
        public static async Task<JToken> LoadFromFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            using (StreamReader file = File.OpenText(filePath))
            using (JsonTextReader reader = new JsonTextReader(file))
                return await JToken.LoadAsync(reader, cancellationToken).ConfigureAwait(false);
        }

        public static async Task<T> LoadFromFileAsync<T>(string filePath, CancellationToken cancellationToken = default)
        {
            JToken json = await LoadFromFileAsync(filePath, cancellationToken).ConfigureAwait(false);
            return json.ToObject<T>();
        }
    }
}
