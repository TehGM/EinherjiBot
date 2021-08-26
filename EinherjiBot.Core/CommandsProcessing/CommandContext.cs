using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    public class CommandContext
    {
        public CommandDescriptor Descriptor { get; }
        private MessageCreateEventArgs MessageEventArgs { get; }

        public DiscordGuild Guild => this.MessageEventArgs.Guild;
        public DiscordUser User => this.MessageEventArgs.Author;
        public string Content => this.Message.Content;
        public DiscordMessage Message => this.MessageEventArgs.Message;
        public DiscordChannel Channel => this.MessageEventArgs.Channel;

        private DiscordMember _guildMember;
        private bool _guildMemberProcessed = false;

        public CommandContext(CommandDescriptor descriptor, MessageCreateEventArgs eventArgs)
        {
            this.Descriptor = descriptor;
            this.MessageEventArgs = eventArgs;
        }

        public async ValueTask<DiscordMember> GetGuildMemberAsync()
        {
            if (this._guildMemberProcessed)
                return this._guildMember;

            new Lazy<DiscordMember>(() => this.Guild != null && this.Guild.Members.TryGetValue(this.User.Id, out var member) ? member : this.Guild?.GetMemberAsync(this.User.Id).ConfigureAwait(false).GetAwaiter().GetResult());
            if (this.Guild == null)
                return SetAndReturn(null);
            if (this.Guild.Members.TryGetValue(this.User.Id, out DiscordMember result))
                return SetAndReturn(result);

            result = await this.Guild.GetMemberAsync(this.User.Id).ConfigureAwait(false);
            return SetAndReturn(result);

            DiscordMember SetAndReturn(DiscordMember value)
            {
                this._guildMember = value;
                this._guildMemberProcessed = true;
                return this._guildMember;
            }
        }
    }
}
