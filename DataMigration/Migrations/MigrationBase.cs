using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using Serilog;

namespace TehGM.EinherjiBot.DataMigration.Migrations
{
    abstract class MigrationBase<TOld, TNew>
    {
        protected ILogger Log { get; }
        private readonly IMongoDatabase _database;
        private readonly string _collectionName;

        private readonly InsertManyOptions _options = new InsertManyOptions()
        {
            BypassDocumentValidation = false
        };

        public MigrationBase(ILogger log, IMongoDatabase database, string collectionName)
        {
            this.Log = log;
            this._database = database;
            this._collectionName = collectionName;
        }

        public async Task RunMigrationAsync(JToken itemsJson, CancellationToken cancellationToken = default)
        {
            Console.WriteLine();
            this.Log.Information("Starting migration {OldEntityType} -> {NewEntityType}", typeof(TOld).Name, typeof(TNew).Name);
            this.Log.Debug("Reading items from JSON");
            List<TOld> oldItems;
            if (itemsJson is JArray jarray)
                oldItems = new List<TOld>(jarray.Children().Select(e => e.ToObject<TOld>()));
            else if (itemsJson is JObject jobj)
                oldItems = new List<TOld>(1) { jobj.ToObject<TOld>() };
            else
                throw new ArgumentException("Items JSON is not of valid type", nameof(itemsJson));
            this.Log.Debug("Found {Count} old items of type {OldEntityType}", oldItems.Count, typeof(TOld).Name);

            List<TNew> newItems = new List<TNew>(oldItems.Count);
            foreach (TOld oldEntity in oldItems)
            {
                TNew newEntity = ConvertEntity(oldEntity);
                if (newEntity != null)
                    newItems.Add(newEntity);
            }

            this.Log.Debug("Opening collection {CollectionName}", this._collectionName);
            IMongoCollection<TNew> collection = _database.GetCollection<TNew>(this._collectionName);
            this.Log.Debug("Inserting {Count} new items of type {OldEntityType}", newItems.Count, typeof(TNew).Name);
            await collection.InsertManyAsync(newItems, _options, cancellationToken).ConfigureAwait(false);
            this.Log.Debug("Finished migration {OldEntityType} -> {NewEntityType}", typeof(TOld).Name, typeof(TNew).Name);
            Console.WriteLine();
        }

        protected abstract TNew ConvertEntity(TOld oldEntity);
    }
}
