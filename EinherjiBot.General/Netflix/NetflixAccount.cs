using System;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot.Netflix
{
    [BsonDiscriminator("NetflixAccount", Required = true)]
    public class NetflixAccount
    {
        [BsonElement("login")]
        public string Login { get; private set; }
        [BsonElement("password")]
        public string Password { get; private set; }
        [BsonElement("modifiedBy")]
        public ulong? ModifiedByID { get; private set; }
        [BsonElement("modifiedTimestamp")]
        public DateTime ModifiedTimestampUTC { get; private set; }

        public void SetLogin(string login, ulong modifiedBy)
        {
            this.Login = login;
            UpdateLastModified(modifiedBy);
        }

        public void SetPassword(string password, ulong modifiedBy)
        {
            this.Password = password;
            UpdateModified(modifiedBy);
        }

        private void UpdateModified(ulong userID)
        {
            this.ModifiedByID = userID;
            this.ModifiedTimestampUTC = DateTime.UtcNow;
        }
    }
}
