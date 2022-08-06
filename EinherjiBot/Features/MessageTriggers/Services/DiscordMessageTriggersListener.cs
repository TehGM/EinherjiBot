using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace TehGM.EinherjiBot.MessageTriggers.Services
{
    public class DiscordMessageTriggersListener : AutostartService, IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;
        private readonly ILogger _log;

        public DiscordMessageTriggersListener(DiscordSocketClient client,
            IServiceProvider services, ILogger<DiscordMessageTriggersListener> log)
        {
            this._client = client;
            this._services = services;
            this._log = log;

            this._client.MessageReceived += this.OnMessageReceivedAsync;
        }

        private Task OnMessageReceivedAsync(SocketMessage message)
        {
            _ = Task.Run(async () =>
            {
                if (message.Channel is not SocketTextChannel guildChannel)
                    return;
                if (message.Source != Discord.MessageSource.User)
                    return;
                if (message.Type != Discord.MessageType.Default)
                    return;
                if (string.IsNullOrWhiteSpace(message.Content))
                    return;


                using IServiceScope botScope = this._services.CreateScope();
                IDiscordAuthProvider auth = botScope.ServiceProvider.GetRequiredService<IDiscordAuthProvider>();
                auth.User = await auth.GetBotContextAsync(base.CancellationToken).ConfigureAwait(false);

                IMessageTriggersProvider provider = botScope.ServiceProvider.GetRequiredService<IMessageTriggersProvider>();
                IEnumerable<MessageTrigger> globalTriggers = await provider.GetGlobalsAsync(base.CancellationToken).ConfigureAwait(false);
                IEnumerable<MessageTrigger> guildTriggers = await provider.GetForGuild(guildChannel.Guild.Id, base.CancellationToken).ConfigureAwait(false);
                IEnumerable<MessageTrigger> triggers = globalTriggers.Union(guildTriggers)
                    .Where(t => t.Actions?.Any() == true);
                if (triggers?.Any() != true)
                    return;

                using IServiceScope scope = this._services.CreateScope();
                IMessageContextProvider messageContext = scope.ServiceProvider.GetRequiredService<IMessageContextProvider>();
                messageContext.Message = message;
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

                    foreach (IMessageTriggerAction action in trigger.Actions)
                    {
                        try
                        {
                            await action.ExecuteAsync(trigger, message, scope.ServiceProvider, base.CancellationToken).ConfigureAwait(false);
                        }
                        catch (Exception ex) when (ex.IsMissingPermissions())
                        {
                            this._log.LogDebug("Failed executing message trigger {TriggerID} action {ActionID} due to missing permissions", trigger.ID, action.ID);
                        }
                        catch (Exception ex) when (ex.LogAsError(this._log, "Failed executing message trigger {TriggerID} action {ActionID}", trigger.ID, action.ID)) { }
                    }
                }
            }, base.CancellationToken);
            return Task.CompletedTask;
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
