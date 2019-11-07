using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TehGM.EinherjiBot.Config;
using TehGM.EinherjiBot.Utilities;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    public abstract class HandlerBase : IDisposable
    {
        protected DiscordSocketClient Client { get; private set; }
        protected BotConfig Config { get; }
        public bool SwitchTaskContext { get; set; } = true;
        protected IList<ICommandProcessor> CommandsStack { get; set; }

        // when handler is constructed, the client might not be connected yet
        // so delay init to first access attempt
        private SocketUser _authorUser;
        public SocketUser AuthorUser
        {
            get
            {
                if (_authorUser == null)
                    _authorUser = Client.GetUser(Config.AuthorID);
                return _authorUser;
            }
        }

        public HandlerBase(DiscordSocketClient client, BotConfig config)
        {
            Logging.Default.Debug("Initializing {HandlerName}", this.GetType().FullName);

            this.Client = client;
            this.Config = config;
            this.CommandsStack = new List<ICommandProcessor>();

            Client.MessageReceived += Client_MessageReceived;
            Client.ReactionAdded += Client_ReactionAdded;
            Client.GuildMemberUpdated += Client_GuildMemberUpdated;
            Client.UserLeft += Client_UserLeft;
            // TODO: more event handlers as they become needed
        }

        protected virtual Task OnMessageReceived(SocketMessage message)
            => ProcessCommandsStackAsync(message);
        protected virtual Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
            => Task.CompletedTask;
        protected virtual Task OnGuildMemberUpdated(SocketGuildUser userBefore, SocketGuildUser userAfter)
            => Task.CompletedTask;
        protected virtual Task OnUserLeft(SocketGuildUser user)
            => Task.CompletedTask;

        protected async Task<bool> ProcessCommandsStackAsync(SocketMessage message)
        {
            for (int i = 0; i < CommandsStack.Count; i++)
            {
                if (await CommandsStack[i].ProcessAsync(Client, message))
                    return true;
            }
            return false;
        }


        private Task Client_MessageReceived(SocketMessage message)
            => InvokeTask(() => OnMessageReceived(message));
        private Task Client_ReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel channel, SocketReaction reaction)
            => InvokeTask(() => OnReactionAdded(message, channel, reaction));
        private Task Client_GuildMemberUpdated(SocketGuildUser arg1, SocketGuildUser arg2)
            => InvokeTask(() => OnGuildMemberUpdated(arg1, arg2));
        private Task Client_UserLeft(SocketGuildUser arg)
            => InvokeTask(() => OnUserLeft(arg));

        private Task InvokeTask(Func<Task> task)
        {
            if (SwitchTaskContext)
            {
                Task.Run(task);
                return Task.CompletedTask;
            }
            return task.Invoke();
        }

        public virtual void Dispose()
        {
            if (Client != null)
            {
                Client.MessageReceived -= Client_MessageReceived;
                Client.ReactionAdded -= Client_ReactionAdded;
                Client.GuildMemberUpdated -= Client_GuildMemberUpdated;
                Client = null;
            }
        }

        public string GetDefaultPrefix()
        {
            string prefix = (CommandVerificator.DefaultPrefixed as CommandVerificator).StringPrefix;
            if (prefix == null)
                prefix = Client.CurrentUser.Mention;
            return prefix;
        }
    }
}
