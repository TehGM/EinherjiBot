using TehGM.EinherjiBot.Caching;

namespace TehGM.EinherjiBot.MessageTriggers.Services
{
    public class MessageTriggersProvider : IMessageTriggersProvider, IDisposable
    {
        private readonly IMessageTriggersStore _store;
        private readonly IEntityCache<Guid, MessageTrigger> _cache;
        private readonly IEntityCache<ulong, MessageTriggersCollection> _guildCache;
        private readonly ILogger _log;
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public MessageTriggersProvider(IMessageTriggersStore store, IEntityCache<Guid, MessageTrigger> cache, IEntityCache<ulong, MessageTriggersCollection> guildCache,
            ILogger<MessageTriggersProvider> log)
        {
            this._store = store;
            this._cache = cache;
            this._guildCache = guildCache;
            this._log = log;
        }

        public async Task<MessageTrigger> GetAsync(Guid id, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (this._cache.TryGet(id, out MessageTrigger result))
                {
                    this._log.LogTrace("Message trigger {ID} found in cache", id);
                    return result;
                }

                result = await this._store.GetAsync(id, cancellationToken).ConfigureAwait(false);
                this._cache.AddOrReplace(result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task<IEnumerable<MessageTrigger>> GetForGuild(ulong guildID, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (this._guildCache.TryGet(guildID, out MessageTriggersCollection results))
                {
                    this._log.LogTrace("Message triggers for guild {ID} found in cache", guildID);
                    return results;
                }

                results = new MessageTriggersCollection(guildID, await this._store.GetForGuildAsync(guildID, cancellationToken).ConfigureAwait(false));
                this._guildCache.AddOrReplace(results);
                return results;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task<IEnumerable<MessageTrigger>> GetGlobalsAsync(CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (this._guildCache.TryGet(MessageTrigger.GlobalGuildID, out MessageTriggersCollection results))
                {
                    this._log.LogTrace("Global message triggers found in cache");
                    return results;
                }

                results = new MessageTriggersCollection(MessageTrigger.GlobalGuildID, await this._store.GetGlobalsAsync(cancellationToken).ConfigureAwait(false));
                this._guildCache.AddOrReplace(results);
                return results;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task UpdateAsync(MessageTrigger trigger, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await this._store.UpdateAsync(trigger, cancellationToken).ConfigureAwait(false);
                this._cache.AddOrReplace(trigger);
                if (!this._guildCache.TryGet(trigger.GuildID, out MessageTriggersCollection guildTriggers))
                    guildTriggers = new MessageTriggersCollection(trigger.GuildID, 1);
                this._guildCache.AddOrReplace(guildTriggers);
            }
            finally
            {
                this._lock.Release();
            }
        }

        public void Dispose()
        {
            try { this._lock?.Dispose(); } catch { }
        }
    }
}
