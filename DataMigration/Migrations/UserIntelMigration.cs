using MongoDB.Driver;
using Serilog;
using TehGM.EinherjiBot.DataMigration.Entities.Old;

namespace TehGM.EinherjiBot.DataMigration.Migrations
{
    using UserData = TehGM.EinherjiBot.DataMigration.Entities.New.UserData;

    class UserIntelMigration : MigrationBase<UserIntel, UserData>
    {
        public UserIntelMigration(ILogger log, IMongoDatabase database, string collectionName) : base(log.ForContext(typeof(UserIntelMigration)), database, collectionName) { }

        protected override UserData ConvertEntity(UserIntel oldEntity)
        {
            base.Log.Debug("Converting UserIntel {UserID}", oldEntity.UserID);
            UserData result = new UserData(oldEntity.UserID);
            result.IsOnline = oldEntity.IsOnline;
            result.StatusChangeTimeUTC = oldEntity.ChangeTimeUTC;
            return result;
        }
    }
}
