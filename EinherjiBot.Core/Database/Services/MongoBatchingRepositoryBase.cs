using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace TehGM.EinherjiBot.Database.Services
{
    public abstract class MongoBatchingRepositoryBase<TKey, TValue> : IBatchingRepository, IDisposable
    {
        // db stuff
        protected MongoDelayedBatchInserter<TKey, TValue> BatchInserter { get; private set; }
        protected IMongoConnection MongoConnection { get; }
        protected IOptionsMonitor<DatabaseOptions> DatabaseOptions { get; }
        // event registrations
        private readonly IDisposable _hostStoppingRegistration;
        // misc
        private readonly ILogger _log;

        public MongoBatchingRepositoryBase(IMongoConnection databaseConnection, IOptionsMonitor<DatabaseOptions> databaseOptions, IHostApplicationLifetime hostLifetime, ILogger log)
        {
            this.DatabaseOptions = databaseOptions;
            this.MongoConnection = databaseConnection;
            this._log = log;

            this._hostStoppingRegistration = hostLifetime.ApplicationStopping.Register(this.FlushBatch);
        }

        protected void RecreateBatchInserter(TimeSpan delay, IMongoCollection<TValue> collection)
        {
            // validate delay is valid
            if (delay <= TimeSpan.Zero)
                throw new ArgumentException("Batching delay must be greater than 0", nameof(delay));

            // flush existing inserter to not lose any changes
            if (this.BatchInserter != null)
                this.BatchInserter.Flush();
            _log?.LogDebug("Creating batch inserter for item type {ItemType} with delay of {Delay}", typeof(TValue).Name, delay);
            this.BatchInserter = new MongoDelayedBatchInserter<TKey, TValue>(delay, _log);
            this.BatchInserter.Collection = collection;
        }

        public void FlushBatch()
            => BatchInserter?.Flush();

        public virtual void Dispose()
        {
            try { this._hostStoppingRegistration?.Dispose(); } catch { }
            try { this.FlushBatch(); } catch { }
            try { this.BatchInserter?.Dispose(); } catch { }
        }
    }
}
