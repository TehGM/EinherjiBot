using MongoDB.Driver;
using Serilog;
using TehGM.EinherjiBot.DataMigration.Entities.New;
using TehGM.EinherjiBot.DataMigration.Entities.Old;

namespace TehGM.EinherjiBot.DataMigration.Migrations
{
    class NetflixAccountMigration : MigrationBase<NetflixAccountInfo, NetflixAccount>
    {
        public NetflixAccountMigration(ILogger log, IMongoDatabase database, string collectionName) : base(log.ForContext(typeof(NetflixAccountMigration)), database, collectionName) { }

        protected override NetflixAccount ConvertEntity(NetflixAccountInfo oldEntity)
        {
            NetflixAccount result = new NetflixAccount();
            result.Login = oldEntity.Login;
            result.Password = oldEntity.Password;
            result.ModifiedByID = oldEntity.LastModifiedByID;
            result.ModifiedTimestampUTC = oldEntity.LastModifiedTimeUtc.UtcDateTime;
            return result;
        }
    }
}
