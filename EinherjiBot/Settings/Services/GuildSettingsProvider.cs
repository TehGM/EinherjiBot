using Discord;
using TehGM.EinherjiBot.Caching;
using TehGM.EinherjiBot.DiscordClient;
using TehGM.EinherjiBot.PlaceholdersEngine;
using TehGM.EinherjiBot.PlaceholdersEngine.Placeholders;

namespace TehGM.EinherjiBot.Settings.Services
{
    public class GuildSettingsProvider : IGuildSettingsProvider
    {
        private readonly IGuildSettingsStore _store;
        private readonly IDiscordClient _client;
        private readonly IDiscordConnection _connection;
        private readonly IPlaceholderSerializer _placeholderSerializer;
        private readonly IEntityCache<ulong, GuildSettings> _cache;
        private readonly ILogger _log;
        private readonly ILockProvider _lock;

        public GuildSettingsProvider(IGuildSettingsStore store, IDiscordClient client, IDiscordConnection connection, IEntityCache<ulong, GuildSettings> cache,
            ILogger<GuildSettingsProvider> log, ILockProvider<GuildSettingsProvider> lockProvider, IPlaceholderSerializer placeholderSerializer)
        {
            this._store = store;
            this._client = client;
            this._connection = connection;
            this._cache = cache;
            this._log = log;
            this._lock = lockProvider;
            this._placeholderSerializer = placeholderSerializer;

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
                    if (result == null)
                        result = await this.CreateDefaultAsync(guildID, cancellationToken).ConfigureAwait(false);
                    this._cache.AddOrReplace(guildID, result);
                }

                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        private async Task<GuildSettings> CreateDefaultAsync(ulong guildID, CancellationToken cancellationToken = default)
        {
            await this._connection.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);
            IGuild guild = await this._client.GetGuildAsync(guildID, CacheMode.AllowDownload, cancellationToken.ToRequestOptions()).ConfigureAwait(false);
            if (guild == null)
                return null;

            // create default
            string userMention = this.BuildDefaultUserPlaceholder(UserDisplayMode.Mention);
            string userName = this.BuildDefaultUserPlaceholder(UserDisplayMode.UsernameWithDiscriminator);
            Color color = (Color)System.Drawing.Color.Cyan;

            GuildSettings result = new GuildSettings(guild.Id);
            result.JoinNotification = new JoinLeaveSettings($"**{userMention}** *(`{userName}`)* **has joined.**") { EmbedColor = color };
            result.LeaveNotification = new JoinLeaveSettings($"**{userMention}** *(`{userName}`)* **has left.**") { EmbedColor = color };
            return result;
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

        private string BuildDefaultUserPlaceholder(UserDisplayMode mode)
        {
            GuildUserDisplayMode guildMode = (GuildUserDisplayMode)(int)mode;
            CurrentUserPlaceholder placeholder = new CurrentUserPlaceholder()
            {
                DisplayMode = guildMode,
                FallbackDisplayMode = mode
            };
            return this._placeholderSerializer.Serialize(placeholder);
        }
    }
}
