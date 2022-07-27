using Newtonsoft.Json;

namespace Blazored.LocalStorage.Serialization
{
    public class NewtonsoftJsonSerializer : IJsonSerializer
    {
        public T Deserialize<T>(string text)
            => JsonConvert.DeserializeObject<T>(text);

        public string Serialize<T>(T obj)
            => JsonConvert.SerializeObject(obj);
    }
}
