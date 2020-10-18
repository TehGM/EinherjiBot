using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot
{
    public static class JsonFileExtensions
    {
        public static async Task<JToken> LoadFromFileAsync(string filePath, CancellationToken cancellationToken = default)
        {
            using (StreamReader file = File.OpenText(filePath))
            using (JsonTextReader reader = new JsonTextReader(file))
                return await JToken.LoadAsync(reader, cancellationToken);
        }

        public static async Task SaveToFileAsync(this JToken data, string filePath, CancellationToken cancellationToken = default)
        {
            using (FileStream file = File.Create(filePath))
            using (StreamWriter wr = new StreamWriter(file))
            using (JsonTextWriter writer = new JsonTextWriter(wr))
                await data.WriteToAsync(writer, cancellationToken);
        }

        public static Task SaveToFileAsync<T>(T data, string filePath, CancellationToken cancellationToken = default)
            => SaveToFileAsync(JToken.FromObject(data), filePath, cancellationToken);
    }
}
