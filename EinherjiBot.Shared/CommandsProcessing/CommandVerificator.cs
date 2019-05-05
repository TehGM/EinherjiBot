using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using Discord.WebSocket;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    public class CommandVerificator : ICommandVerificator
    {
        public static ICommandVerificator DefaultPrefixed { get; } = new CommandVerificator()
        {
            IgnoreBots = true,
            AcceptMentionPrefix = true,
            AcceptGuildMessages = true,
            AcceptPrivateMessages = true,
            StringPrefix = "."
        };
        public static ICommandVerificator DefaultPrefixedGuildOnly { get; } = new CommandVerificator()
        {
            IgnoreBots = true,
            AcceptMentionPrefix = true,
            AcceptGuildMessages = true,
            AcceptPrivateMessages = false,
            StringPrefix = "."
        };

        public bool IgnoreBots { get; set; }
        public bool AcceptMentionPrefix { get; set; }
        public string StringPrefix { get; set; }

        public bool AcceptGuildMessages { get; set; }
        public bool AcceptPrivateMessages { get; set; }

        public bool RequirePrefix => AcceptMentionPrefix || !string.IsNullOrWhiteSpace(StringPrefix);

        public bool Verify(SocketCommandContext command, out string actualCommand)
        {
            actualCommand = null;
            if (IgnoreBots && (command.User.IsBot || command.User.IsWebhook))
                return false;
            if (!AcceptGuildMessages && command.Guild != null)
                return false;
            if (!AcceptPrivateMessages && command.IsPrivate)
                return false;
            if (!RequirePrefix)
            {
                actualCommand = command.Message.Content;
                return true;
            }
            // extract actual command so it can be confirmed with regex
            int cmdIndex = 0;
            if ((AcceptMentionPrefix && command.Message.HasMentionPrefix(command.Client.CurrentUser, ref cmdIndex)) ||
                (!string.IsNullOrWhiteSpace(StringPrefix) && command.Message.HasStringPrefix(StringPrefix, ref cmdIndex)))
            {
                actualCommand = command.Message.Content.Substring(cmdIndex);
                return true;
            }
            return false;
        }
    }
}
