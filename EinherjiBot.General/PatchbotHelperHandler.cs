using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TehGM.EinherjiBot.CommandsProcessing;
using TehGM.EinherjiBot.Config;
using TehGM.EinherjiBot.DataModels;
using TehGM.EinherjiBot.Extensions;

namespace TehGM.EinherjiBot
{
    class PatchbotHelperHandler : HandlerBase
    {
        public PatchbotHelperHandler(DiscordSocketClient client, BotConfig config) : base(client, config)
        {
        }

        protected override Task OnMessageReceived(SocketMessage message)
        {
            if (Config.Data.PatchbotHelper.PatchbotIDs.Contains(message.Author.Id))
                return ProcessPatchbotMessageAsync(message);
            return ProcessCommandsStackAsync(message);
        }

        private async Task ProcessPatchbotMessageAsync(SocketMessage message)
        {
            if (!(message.Channel is SocketTextChannel channel))
                return;
            if (message.Embeds.Count == 0)
                return;

            // get game from embed author text
            Embed embed = message.Embeds.First();
            string gameName = embed.Author?.Name;
            if (string.IsNullOrEmpty(gameName))
                return;
            PatchbotHelperGame game = Config.Data.PatchbotHelper.FindGame(gameName);

            // if no one subscribes to this game, abort
            if (game.SubscribersIDs.Count == 0)
                return;

            // get only subscribers that are present in this channel
            IEnumerable<SocketGuildUser> presentSubscribers = channel.Users.Where(user => game.SubscribersIDs.Contains(user.Id));

            // ping them all
            await message.ReplyAsync($"{string.Join(' ', presentSubscribers.Select(user => user.Mention))}\n{message.GetJumpUrl()}");
        }
    }
}
