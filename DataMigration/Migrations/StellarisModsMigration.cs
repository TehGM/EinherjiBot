using MongoDB.Driver;
using Serilog;
using TehGM.EinherjiBot.DataMigration.Entities.Old;
using TehGM.EinherjiBot.Stellaris;

namespace TehGM.EinherjiBot.DataMigration.Migrations
{
    class StellarisModsMigration : MigrationBase<StellarisModInfo, StellarisMod>
    {
        public StellarisModsMigration(ILogger log, IMongoDatabase database, string collectionName) : base(log.ForContext(typeof(StellarisModsMigration)), database, collectionName) { }

        protected override StellarisMod ConvertEntity(StellarisModInfo oldEntity)
        {
            base.Log.Debug("Converting StellarisModInfo {ModName}", oldEntity.Name);
            return new StellarisMod(oldEntity.Name, oldEntity.URL);
        }
    }
}
