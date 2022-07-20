using Microsoft.Extensions.Hosting;
using MongoDB.Driver;
using TehGM.EinherjiBot.Database.Services;

namespace TehGM.EinherjiBot.Database
{
    public abstract class MongoBatchingRepositoryBase<TKey, TValue> : IBatchingRepository, IDisposable
    {
        protected MongoDelayedBatchInserter<TKey, TValue> BatchInserter { get; private set; }
        protected IMongoConnection MongoConnection { get; }
        protected IOptionsMonitor<MongoOptions> DatabaseOptions { get; }

        private readonly IDisposable _hostStoppingRegistration;
        private readonly ILogger _log;

        protected abstract IMongoCollection<TValue> Collection { get; }
        protected abstract TimeSpan BatchDelay { get; }
        protected virtual EqualityComparer<TKey> KeyEqualityComparer => EqualityComparer<TKey>.Default;

        public MongoBatchingRepositoryBase(IMongoConnection databaseConnection, IOptionsMonitor<MongoOptions> databaseOptions, IHostApplicationLifetime hostLifetime, ILogger log)
        {
            this.DatabaseOptions = databaseOptions;
            this.MongoConnection = databaseConnection;
            this._log = log;

            this._hostStoppingRegistration = hostLifetime.ApplicationStopping.Register(this.FlushBatch);

            this.InitializeBatchInserter();
        }

        protected void InitializeBatchInserter()
        {
            TimeSpan delay = this.BatchDelay;

            // validate delay is valid
            if (delay <= TimeSpan.Zero)
                throw new ArgumentException("Batching delay must be greater than 0", nameof(BatchDelay));

            // flush existing inserter to not lose any changes
            if (this.BatchInserter != null)
                this.BatchInserter.Flush();

            this._log?.LogDebug("Creating batch inserter for item type {ItemType} with delay of {Delay}", typeof(TValue).Name, delay);
            this.BatchInserter = new MongoDelayedBatchInserter<TKey, TValue>(delay, this.Collection, this.KeyEqualityComparer, this._log);
        }

        public void FlushBatch()
            => this.BatchInserter?.Flush();

        public Task FlushBatchAsync()
            => this.BatchInserter?.FlushAsync();

        public virtual void Dispose()
        {
            try { this._hostStoppingRegistration?.Dispose(); } catch { }
            try { this.FlushBatchAsync().GetAwaiter().GetResult(); } catch { }
            try { this.BatchInserter?.Dispose(); } catch { }
        }
    }
}
