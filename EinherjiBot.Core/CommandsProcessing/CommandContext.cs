using System.Collections.Generic;
using System.Linq;
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

        public Task<DiscordMessage> ReplyAsync(string content, DiscordEmbed embed = null)
            => this.ReplyAsync(content, embed, Mentions.All);

        public Task<DiscordMessage> ReplyAsync(string content, DiscordEmbed embed, IEnumerable<IMention> mentions)
        {
            return this.CreateBuilder(content, embed, mentions)
                .SendAsync(this.Channel);
        }

        public Task<DiscordMessage> InlineReplyAsync(string content, DiscordEmbed embed = null)
            => this.InlineReplyAsync(content, embed, Mentions.All);

        public Task<DiscordMessage> InlineReplyAsync(string content, DiscordEmbed embed, IEnumerable<IMention> mentions)
        {
            DiscordMessageBuilder builder = this.CreateBuilder(content, embed, mentions);
            builder.WithReply(this.Message.Id, true);
            return builder.SendAsync(this.Channel);
        }

        private DiscordMessageBuilder CreateBuilder(string content, DiscordEmbed embed, IEnumerable<IMention> mentions)
        {
            DiscordMessageBuilder builder = new DiscordMessageBuilder();
            builder.Content = content;
            builder.WithEmbed(embed);
            if (mentions?.Any() == true)
                builder.WithAllowedMentions(mentions);
            return builder;
        }
    }
}
