using TehGM.EinherjiBot.Caching;

namespace TehGM.EinherjiBot.Settings.Services
{
    public class GuildSettingsProvider : IGuildSettingsProvider
    {
        private readonly IGuildSettingsStore _store;
        private readonly IEntityCache<ulong, GuildSettings> _cache;
        private readonly ILogger _log;
        private readonly ILockProvider _lock;

        public GuildSettingsProvider(IGuildSettingsStore store, IEntityCache<ulong, GuildSettings> cache, 
            ILogger<GuildSettingsProvider> log, ILockProvider<GuildSettingsProvider> lockProvider)
        {
            this._store = store;
            this._cache = cache;
            this._log = log;
            this._lock = lockProvider;

            this._cache.DefaultExpiration = new SlidingEntityExpiration(TimeSpan.FromMinutes(10));
        }

        public async Task<GuildSettings> GetAsync(ulong guildID, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                if (this._cache.TryGet(guildID, out GuildSettings result))
                    this._log.LogTrace("Settings for guild {GuildID} found in cache", guildID);
                else
                {
                    result = await this._store.GetAsync(guildID, cancellationToken).ConfigureAwait(false);
                    this._cache.AddOrReplace(guildID, result);
                }

                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        public async Task AddOrUpdateAsync(GuildSettings setting, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                await this._store.UpdateAsync(setting, cancellationToken).ConfigureAwait(false);
                this._cache.AddOrReplace(setting);
            }
            finally
            {
                this._lock.Release();
            }
        }
    }
}
