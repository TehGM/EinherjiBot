using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace TehGM.EinherjiBot.Intel.Services
{
    class StatusChecker : IStatusChecker, IDisposable
    {
        private readonly DiscordClient _client;
        private readonly Dictionary<ulong, UserStatus?> _requestedUsers = new Dictionary<ulong, UserStatus?>();
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

        public StatusChecker(DiscordClient client)
        {
            this._client = client;

            this._client.GuildCreated += OnGuildJoinedAsync;
            this._client.GuildMemberAdded += OnMemberJoinedAsync;
            this._client.GuildMemberRemoved += OnMemberLeftAsync;
        }

        public async Task<UserStatus?> GetStatusAsync(ulong userID)
        {
            await this._lock.WaitAsync().ConfigureAwait(false);
            try
            {
                // get the user object to check if it has presence already
                DiscordUser user = await this._client.GetUserAsync(userID).ConfigureAwait(false);
                if (user.Presence != null)
                    return user.Presence.Status;

                // if user has already been tried, return the previously determined value
                if (this._requestedUsers.TryGetValue(userID, out UserStatus? result))
                    return result;

                // check each guild the bot is in, and if any found, assume user just offline
                // else claim status unknown
                DiscordGuild guild = await this.ScanGuildsAsync(user).ConfigureAwait(false);
                if (guild != null)
                    result = UserStatus.Offline;
                else
                    result = null;

                // regardless of the result, mark the user ID as already requested
                this._requestedUsers.Add(userID, result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }

        private async Task<DiscordGuild> ScanGuildsAsync(DiscordUser user)
        {
            foreach (ulong guildID in this._client.Guilds.Keys)
            {
                DiscordGuild guild = await this._client.GetGuildAsync(guildID).ConfigureAwait(false);

                // check if user is in the guild
                DiscordMember member = await guild.GetMemberSafeAsync(user.Id).ConfigureAwait(false);
                if (member != null)
                    return guild;
            }
            return null;
        }

        private async Task OnGuildJoinedAsync(DiscordClient sender, GuildCreateEventArgs e)
        {
            await this._lock.WaitAsync().ConfigureAwait(false);
            try
            {
                // when joining a new guild, clear up all the users that are already requested
                // this will allow the provider to get new presences
                foreach (ulong memberID in e.Guild.Members.Keys)
                    this._requestedUsers.Remove(memberID);
            }
            finally
            {
                this._lock.Release();
            }
        }

        private async Task OnMemberJoinedAsync(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            await this._lock.WaitAsync().ConfigureAwait(false);
            try
            {
                // when the user joins, remove them from the requested users as they can now have their presence retrieved
                this._requestedUsers.Remove(e.Member.Id);
            }
            finally
            {
                this._lock.Release();
            }
        }

        private async Task OnMemberLeftAsync(DiscordClient sender, GuildMemberRemoveEventArgs e)
        {
            await this._lock.WaitAsync().ConfigureAwait(false);
            try
            {
                // when the user leaves, remove them but only if they aren't in any of the guilds anymore
                // also skip the scan if the user wasn't requested, as it'd be pretty pointless
                if (!this._requestedUsers.ContainsKey(e.Member.Id))
                    return;
                DiscordGuild guild = await this.ScanGuildsAsync(e.Member).ConfigureAwait(false);
                if (guild == null)
                    this._requestedUsers.Remove(e.Member.Id);
            }
            finally
            {
                this._lock.Release();
            }
        }

        public void Dispose()
        {
            try { this._client.GuildCreated -= OnGuildJoinedAsync; } catch { }
            try { this._client.GuildMemberAdded -= OnMemberJoinedAsync; } catch { }
            try { this._client.GuildMemberRemoved -= OnMemberLeftAsync; } catch { }
            try { this._lock?.Dispose(); } catch { }
            this._requestedUsers.Clear();
        }
    }
}
