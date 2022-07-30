﻿using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using TehGM.EinherjiBot.PlaceholdersEngine;

namespace TehGM.EinherjiBot.MessageTriggers.Services
{
    public class DiscordMessageTriggersListener : AutostartService, IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IMessageTriggersProvider _provider;
        private readonly IPlaceholdersEngine _placeholders;
        private readonly IServiceProvider _services;
        private readonly ILogger _log;

        public DiscordMessageTriggersListener(DiscordSocketClient client, IMessageTriggersProvider provider, IPlaceholdersEngine placeholders,
            IServiceProvider services, ILogger<DiscordMessageTriggersListener> log)
        {
            this._client = client;
            this._provider = provider;
            this._placeholders = placeholders;
            this._services = services;
            this._log = log;

            this._client.MessageReceived += this.OnMessageReceivedAsync;
        }

        private async Task OnMessageReceivedAsync(SocketMessage message)
        {
            if (message.Channel is not SocketTextChannel guildChannel)
                return;
            if (message.Source != Discord.MessageSource.User)
                return;
            if (message.Type != Discord.MessageType.Default)
                return;
            if (string.IsNullOrWhiteSpace(message.Content))
                return;

            IEnumerable<MessageTrigger> globalTriggers = await this._provider.GetGlobalsAsync(base.CancellationToken).ConfigureAwait(false);
            IEnumerable<MessageTrigger> guildTriggers = await this._provider.GetForGuild(guildChannel.Guild.Id, base.CancellationToken).ConfigureAwait(false);
            IEnumerable<MessageTrigger> triggers = globalTriggers.Union(guildTriggers);
            if (triggers?.Any() != true)
                return;

            using IServiceScope scope = this._services.CreateScope();
            IDiscordAuthProvider authProvider = scope.ServiceProvider.GetRequiredService<IDiscordAuthProvider>();
            IDiscordAuthContext authContext = await authProvider.FromMessageAsync(message, base.CancellationToken).ConfigureAwait(false);
            authProvider.User = authContext;

            SocketGuildUser user = guildChannel.Guild.GetUser(message.Author.Id);

            foreach (MessageTrigger trigger in triggers)
            {
                if (!this.CheckFilters(trigger.Filters, guildChannel, user))
                    return;
                if (!trigger.IsMatch(message.Content))
                    continue;

                string response = await this._placeholders.ConvertPlaceholdersAsync(trigger.Response, scope.ServiceProvider, base.CancellationToken).ConfigureAwait(false);
                try
                {
                    await guildChannel.SendMessageAsync(response,
                        options: base.CancellationToken.ToRequestOptions()).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex.IsMissingPermissions())
                {
                    this._log.LogDebug("Failed running message trigger {ID} due to missing permissions", trigger.ID);
                }
            }
        }

        private bool CheckFilters(MessageTriggerFilters filters, SocketTextChannel channel, SocketGuildUser author)
        {
            bool result =
                CheckWhitelistFilter(filters.WhitelistedGuildIDs, channel.Guild.Id) && CheckBlacklistFilter(filters.BlacklistedGuildIDs, channel.Guild.Id) &&
                CheckWhitelistFilter(filters.WhitelistedChannelIDs, channel.Id) && CheckBlacklistFilter(filters.BlacklistedChannelIDs, channel.Id) &&
                CheckWhitelistFilter(filters.WhitelistedUserIDs, author.Id) && CheckBlacklistFilter(filters.BlacklistedUserIDs, author.Id);
            if (!result)
                return false;

            // roles need special treatment as user might have a number of them
            if (filters.WhitelistedRoleIDs == null && filters.BlacklistedRoleIDs == null)
                return true;
            foreach (SocketRole role in author.Roles)
            {
                if (!CheckWhitelistFilter(filters.WhitelistedRoleIDs, role.Id) || !CheckBlacklistFilter(filters.BlacklistedRoleIDs, role.Id))
                    return false;
            }

            return true;

            bool CheckWhitelistFilter(ICollection<ulong> filter, ulong id)
                => filter == null || filter.Contains(id);
            bool CheckBlacklistFilter(ICollection<ulong> filter, ulong id)
                => filter == null || !filter.Contains(id);
        }

        public override void Dispose()
        {
            try { this._client.MessageReceived -= this.OnMessageReceivedAsync; } catch { }
            base.Dispose();
        }
    }
}
