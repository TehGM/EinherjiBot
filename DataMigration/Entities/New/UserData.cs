using System;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.DataMigration.Entities.New
{
    class UserData
    {
        [BsonId]
        public ulong ID { get; }
        [BsonElement("onlineStatus")]
        public bool IsOnline { get; set; }
        [BsonElement("onlineStatusChangeTimestamp")]
        public DateTime? StatusChangeTimeUTC { get; set; }

        [BsonConstructor(nameof(ID))]
        public UserData(ulong id)
        {
            this.ID = id;
        }
    }
}
