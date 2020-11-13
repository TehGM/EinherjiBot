using MongoDB.Driver;
using Serilog;
using TehGM.EinherjiBot.DataMigration.Entities.Old;
using TehGM.EinherjiBot.Patchbot;
using System.Collections.Generic;

namespace TehGM.EinherjiBot.DataMigration.Migrations
{
    class PatchbotGamesMigration : MigrationBase<PatchbotHelperGame, PatchbotGame>
    {
        public PatchbotGamesMigration(ILogger log, IMongoDatabase database, string collectionName) : base(log.ForContext(typeof(PatchbotGamesMigration)), database, collectionName) { }

        protected override PatchbotGame ConvertEntity(PatchbotHelperGame oldEntity)
        {
            base.Log.Debug("Converting PatchbotHelperGame {GameName}", oldEntity.Name);
            PatchbotGame result = new PatchbotGame(oldEntity.Name, oldEntity.Aliases);
            foreach (ulong sub in oldEntity.SubscribersIDs ?? new HashSet<ulong>())
                result.SubscriberIDs.Add(sub);
            return result;
        }
    }
}
