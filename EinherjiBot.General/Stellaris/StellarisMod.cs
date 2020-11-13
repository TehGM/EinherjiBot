using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Stellaris
{
    public class StellarisMod
    {
        [BsonId]
        public ObjectId ID { get; }
        [BsonElement("Name")]
        public string Name { get; }
        [BsonElement("URL")]
        public string URL { get; }

        [BsonConstructor(nameof(ID), nameof(Name), nameof(URL))]
        private StellarisMod(ObjectId id, string name, string url)
        {
            this.ID = id;
            this.Name = name;
            this.URL = url;
        }

        public StellarisMod(string name, string url)
            : this(ObjectId.GenerateNewId(), name, url) { }
    }
}
