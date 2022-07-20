using System.Linq.Expressions;
using MongoDB.Driver;

namespace TehGM.EinherjiBot.Database.Services
{
    public class MongoDelayedBatchInserter<TKey, TItem> : IDisposable
    {
        public IMongoCollection<TItem> Collection { get; }
        public ReplaceOptions DefaultReplaceOptions { get; }

        private readonly TimeSpan _delay;
        private readonly IDictionary<TKey, MongoDelayedInsert<TItem>> _batchedInserts;
        private TaskCompletionSource<object> _batchTcs;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private readonly ILogger _log;

        public bool HasPendingInserts => this._batchedInserts.Count != 0;

        public MongoDelayedBatchInserter(TimeSpan delay, IMongoCollection<TItem> collection, IEqualityComparer<TKey> comparer, ILogger log = null)
        {
            if (delay <= TimeSpan.Zero)
                throw new ArgumentException("Batching delay needs to be non-zero positive value", nameof(delay));
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            this._delay = delay;
            this._log = log;
            this.Collection = collection;
            this._batchedInserts = new Dictionary<TKey, MongoDelayedInsert<TItem>>(comparer);
            this.DefaultReplaceOptions = new ReplaceOptions() { IsUpsert = true, BypassDocumentValidation = false };
        }

        public MongoDelayedBatchInserter(TimeSpan delay, IMongoCollection<TItem> collection, ILogger log = null)
            : this(delay, collection, EqualityComparer<TKey>.Default, log) { }

        public async Task BatchAsync(TKey key, MongoDelayedInsert<TItem> item, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this._batchedInserts[key] = item;
                if (this._batchTcs != null)
                    return;
                _ = this.BatchDelayAsync();
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task UnbatchAsync(TKey key, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this._batchedInserts.Remove(key);
                if (this._batchedInserts.Count == 0)
                    this.CancelDelay();
            }
            finally
            {
                this._lock.Release();
            }
        }

        public void Flush()
        {
            this._batchTcs?.TrySetResult(null);
        }

        public Task FlushAsync()
        {
            this.CancelDelay();
            return this.FlushInternalAsync();
        }

        private async Task FlushInternalAsync()
        {
            await this._lock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!this.HasPendingInserts)
                    return;

                int batchCount = this._batchedInserts.Count;
                this._log?.LogTrace("Beginning batch flush. {BatchedCount} items of type {ItemType} queued.", batchCount, typeof(TItem).Name);
                IEnumerable<WriteModel<TItem>> batch = this._batchedInserts.Values.Select(i => this.ConvertToModel(i));
                await this.Collection.BulkWriteAsync(batch, new BulkWriteOptions() { IsOrdered = true }).ConfigureAwait(false);
                this._batchedInserts.Clear();
                this._log?.LogDebug("Batch flushed. {BatchedCount} items of type {ItemType} added to the database.", batchCount, typeof(TItem).Name);
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) when (ex.LogAsError(this._log, "Error occured when flushing a batch"))
            {
                throw;
            }
            finally
            {
                this._batchTcs = null;
                this._lock.Release();
            }
        }

        private async Task BatchDelayAsync()
        {
            this._batchTcs = new TaskCompletionSource<object>();
            Task delayTask = Task.Delay(this._delay);

            try
            {
                await Task.WhenAny(this._batchTcs.Task, delayTask).ConfigureAwait(false);
                await this.FlushAsync().ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            finally
            {
                this._batchTcs = null;
            }
        }

        private void CancelDelay()
        {
            this._batchTcs?.TrySetCanceled(CancellationToken.None);
        }

        private WriteModel<TItem> ConvertToModel(MongoDelayedInsert<TItem> insert)
        {
            if (insert is MongoDelayedUpsert<TItem> upsert)
            {
                ReplaceOptions options = upsert.ReplaceOptions ?? this.DefaultReplaceOptions;
                return new ReplaceOneModel<TItem>(upsert.Filter, upsert.Item)
                {
                    IsUpsert = options.IsUpsert,
                    Collation = options.Collation,
                    Hint = options.Hint
                };
            }
            else
                return new InsertOneModel<TItem>(insert.Item);
        }

        public void Dispose()
        {
            this._batchTcs?.TrySetCanceled();
            this._lock?.Dispose();
        }
    }

    public class MongoDelayedUpsert<T> : MongoDelayedInsert<T>
    {
        public readonly Expression<Func<T, bool>> Filter;
        public readonly ReplaceOptions ReplaceOptions;

        public MongoDelayedUpsert(Expression<Func<T, bool>> filter, T item, ReplaceOptions replaceOptions)
            : base(item)
        {
            this.Filter = filter;
            this.ReplaceOptions = replaceOptions;
        }
    }

    public class MongoDelayedInsert<T>
    {
        public readonly T Item;

        public MongoDelayedInsert(T item)
        {
            this.Item = item;
        }
    }
}
