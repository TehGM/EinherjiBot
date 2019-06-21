﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Rest;

namespace TehGM.EinherjiBot.Extensions
{
    public static class SocketMessageExtensions
    {
        public static Task<RestUserMessage> ReplyAsync(this SocketCommandContext message, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => ReplyAsync(message.Message, text, isTTS, embed, options);

        public static Task<RestUserMessage> ReplyAsync(this SocketMessage message, string text = null, bool isTTS = false, Embed embed = null, RequestOptions options = null)
            => message.Channel.SendMessageAsync(text, isTTS, embed, options);
    }
}
