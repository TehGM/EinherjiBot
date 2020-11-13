using System;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.DataMigration.Entities.New
{
    [BsonDiscriminator("NetflixAccount", Required = true)]
    public class NetflixAccount
    {
        [BsonElement("login")]
        public string Login { get; set; }
        [BsonElement("password")]
        public string Password { get; set; }
        [BsonElement("modifiedBy")]
        public ulong? ModifiedByID { get; set; }
        [BsonElement("modifiedTimestamp")]
        public DateTime ModifiedTimestampUTC { get; set; }
    }
}
