﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace TehGM.EinherjiBot.Database.Services
{
    public class MongoDelayedBatchInserter<TKey, TItem> : IDisposable
    {
        public IMongoCollection<TItem> Collection { get; set; }
        public ReplaceOptions DefaultReplaceOptions { get; }

        private readonly TimeSpan _delay;
        private readonly IDictionary<TKey, MongoDelayedInsert<TItem>> _batchedInserts;
        private TaskCompletionSource<object> _batchTcs;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        private readonly ILogger _log;

        public MongoDelayedBatchInserter(TimeSpan delay, IEqualityComparer<TKey> comparer, ILogger log = null)
        {
            this._delay = delay;
            this._log = log;
            this._batchedInserts = new Dictionary<TKey, MongoDelayedInsert<TItem>>(comparer);
            this.DefaultReplaceOptions = new ReplaceOptions() { IsUpsert = true, BypassDocumentValidation = false };
        }

        public MongoDelayedBatchInserter(TimeSpan delay, ILogger log = null)
            : this(delay, EqualityComparer<TKey>.Default, log) { }

        public async Task BatchAsync(TKey key, MongoDelayedInsert<TItem> item, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this._batchedInserts[key] = item;
                if (_batchTcs != null)
                    return;
                _ = BatchDelayAsync();
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task UnbatchAsync(TKey key, CancellationToken cancellationToken = default)
        {
            await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                this._batchedInserts.Remove(key);
                if (_batchedInserts.Count == 0)
                    _batchTcs?.TrySetCanceled();
            }
            finally
            {
                _lock.Release();
            }
        }

        public void Flush()
        {
            _batchTcs?.TrySetResult(null);
        }

        private async Task BatchDelayAsync()
        {
            _batchTcs = new TaskCompletionSource<object>();
            Task delayTask = Task.Delay(this._delay);

            try
            {
                await Task.WhenAny(_batchTcs.Task, delayTask).ConfigureAwait(false);

                await _lock.WaitAsync().ConfigureAwait(false);
                try
                {
                    int batchCount = _batchedInserts.Count;
                    _log?.LogTrace("Beginning batch flush. {BatchedCount} items of type {ItemType} queued.", batchCount, typeof(TItem).Name);
                    foreach (KeyValuePair<TKey, MongoDelayedInsert<TItem>> inserts in _batchedInserts)
                        await this.Collection.ReplaceOneAsync(inserts.Value.Filter, inserts.Value.Item, inserts.Value.ReplaceOptions ?? this.DefaultReplaceOptions).ConfigureAwait(false);
                    _batchedInserts.Clear();
                    _log?.LogDebug("Batch flushed. {BatchedCount} items of type {ItemType} added to the database.", batchCount, typeof(TItem).Name);
                }
                catch (OperationCanceledException) { }
                catch (Exception ex) when (ex.LogAsError(_log, "Error occured when flushing a batch"))
                {
                    throw;
                }
                finally
                {
                    _batchTcs = null;
                    _lock.Release();
                }
            }
            catch (OperationCanceledException) { }
            finally
            {
                _batchTcs = null;
            }
        }

        public void Dispose()
        {
            _batchTcs?.TrySetCanceled();
            _lock?.Dispose();
        }
    }

    public class MongoDelayedInsert<T>
    {
        public readonly Expression<Func<T, bool>> Filter;
        public readonly T Item;
        public ReplaceOptions ReplaceOptions;

        public MongoDelayedInsert(Expression<Func<T, bool>> filter, T item, ReplaceOptions replaceOptions)
        {
            this.Filter = filter;
            this.Item = item;
            this.ReplaceOptions = replaceOptions;
        }
    }
}
