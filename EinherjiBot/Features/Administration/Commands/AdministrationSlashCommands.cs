using Discord;
using Discord.Interactions;
using TehGM.EinherjiBot.Auditing;

namespace TehGM.EinherjiBot.Administration.Commands
{
    public class AdministrationSlashCommands : EinherjiInteractionModule
    {
        private readonly IAuditStore<CommandAuditEntry> _audit;

        public AdministrationSlashCommands(IAuditStore<CommandAuditEntry> audit)
        {
            this._audit = audit;
        }

        [SlashCommand("purge", "Removes last X messages in current channel", true)]
        [RequireUserPermission(ChannelPermission.ManageMessages)]
        [RequireBotPermission(ChannelPermission.ManageMessages)]
        [EnabledInDm(false)]
        public async Task PurgeAsync(
            [Summary("Count", "Count of messages to delete")] uint count)
        {
            await base.DeferAsync(true, base.GetRequestOptions()).ConfigureAwait(false);
            IEnumerable<IMessage> msgs = await base.Context.Channel.GetMessagesAsync((int)count, base.CancellationToken).FlattenAsync().ConfigureAwait(false);

            // bulk can only delete messages not older than 2 weeks
            DateTimeOffset bulkMaxAge = DateTimeOffset.UtcNow - TimeSpan.FromDays(14) - TimeSpan.FromSeconds(3);
            IEnumerable<IMessage> newerMessages = msgs.Where(msg => msg.Timestamp >= bulkMaxAge);
            IEnumerable<IMessage> olderMessages = msgs.Except(newerMessages);
            int olderCount = olderMessages.Count();

            await (base.Context.Channel as ITextChannel).DeleteMessagesAsync(newerMessages, base.Context.CancellationToken).ConfigureAwait(false);
            await this._audit.AddAuditAsync(new CommandAuditEntry(base.Context, "purge", new Dictionary<string, object>() { { "count", count } }), base.CancellationToken).ConfigureAwait(false);

            _ = Task.Run(async () =>
            {
                if (olderCount > 0)
                {
                    await base.ModifyOriginalResponseAsync(msg => msg.Content = $"{olderCount} messages are older than 2 weeks, which makes it impossible to delete them quickly due to Discord limitations. This might take a while.",
                        base.GetRequestOptions()).ConfigureAwait(false);

                    foreach (IMessage msg in olderMessages)
                    {
                        await Task.Delay(1000).ConfigureAwait(false);
                        await base.Context.Channel.DeleteMessageAsync(msg, base.GetRequestOptions()).ConfigureAwait(false);
                    }
                }
                await base.ModifyOriginalResponseAsync(msg => msg.Content = $"{EinherjiEmote.SuccessSymbol} {msgs.Count()} previous message{(msgs.Count() > 1 ? "s were" : " was")} taken down.",
                    base.GetRequestOptions()).ConfigureAwait(false);
            }, base.CancellationToken);
        }
    }
}
