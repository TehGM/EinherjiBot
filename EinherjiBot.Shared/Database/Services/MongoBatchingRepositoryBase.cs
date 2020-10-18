using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace TehGM.EinherjiBot.Database.Services
{
    public abstract class MongoBatchingRepositoryBase<TKey, TValue> : IBatchingRepository, IDisposable
    {
        protected abstract TimeSpan BatchDelay { get; set; }
        protected abstract IMongoCollection<TValue> Collection { get; set; }

        // db stuff
        protected MongoDelayedBatchInserter<TKey, TValue> BatchInserter { get; private set; }
        protected IMongoConnection MongoConnection { get; }
        protected IOptionsMonitor<DatabaseOptions> DatabaseOptions { get; }
        // event registrations
        private readonly IDisposable _hostStoppingRegistration;
        private readonly IDisposable _configChangeRegistration;
        // misc
        private readonly ILogger _log;

        public MongoBatchingRepositoryBase(IMongoConnection databaseConnection, IOptionsMonitor<DatabaseOptions> databaseOptions, IHostApplicationLifetime hostLifetime, ILogger log)
        {
            this.DatabaseOptions = databaseOptions;
            this.MongoConnection = databaseConnection;
            this._log = log;

            this._hostStoppingRegistration = hostLifetime.ApplicationStopping.Register(this.BatchInserter.Flush);
            this._configChangeRegistration = this.DatabaseOptions.OnChange(_ => RecreateBatchInserter());

            this.RecreateBatchInserter();
        }

        protected void RecreateBatchInserter()
        {
            // validate delay is valid
            if (BatchDelay <= TimeSpan.Zero)
                throw new ArgumentException("Batching delay must be greater than 0", nameof(BatchDelay));

            // flush existing inserter to not lose any changes
            if (this.BatchInserter != null)
                this.BatchInserter.Flush();
            _log?.LogDebug("Creating batch inserter for item type {ItemType} with delay of {Delay}", typeof(TValue).Name, BatchDelay);
            this.BatchInserter = new MongoDelayedBatchInserter<TKey, TValue>(BatchDelay, _log);
            this.BatchInserter.Collection = Collection;
        }

        public void FlushBatch()
            => BatchInserter?.Flush();

        public virtual void Dispose()
        {
            this._configChangeRegistration?.Dispose();
            this._hostStoppingRegistration?.Dispose();
            this.BatchInserter?.Flush();
            this.BatchInserter?.Dispose();
        }
    }
}
