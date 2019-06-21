using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
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
            CommandsStack.Add(new RegexUserCommand("^patchbot sub(?:scribe)?(?: (.+))?", CmdSubscribe));
            CommandsStack.Add(new RegexUserCommand("^patchbot unsub(?:scribe)?(?: (.+))?", CmdUnsubscribe));
            CommandsStack.Add(new RegexUserCommand("^patchbot add id(?: (\\d+))?", CmdAddID));
            CommandsStack.Add(new RegexUserCommand("^patchbot (?:remove|del|delete) id(?: (\\d+))?", CmdRemoveID));
        }

        protected override Task OnMessageReceived(SocketMessage message)
        {
            if (Config.Data.PatchbotHelper.PatchbotIDs?.Contains(message.Author.Id) == true)
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

        private async Task CmdSubscribe(SocketCommandContext message, Match match)
        {
            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await message.ReplyAsync("\u274C Please specify the game name to subscribe.");
                return;
            }
            string gameName = match.Groups[1].Value.Trim();
            PatchbotHelperGame game = Config.Data.PatchbotHelper.FindGame(gameName);
            if (game == null)
            {
                await SendGameNotFoundAsync(message.Channel, gameName);
                return;
            }
            if (game.AddSubscriber(message.User.Id))
                await Config.Data.SaveAsync();
            await message.ReplyAsync($"\u2705 You will now get pinged about `{game.Name}` updates.");
        }
        private async Task CmdUnsubscribe(SocketCommandContext message, Match match)
        {
            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await message.ReplyAsync("\u274C Please specify the game name to unsubscribe.");
                return;
            }
            string gameName = match.Groups[1].Value.Trim();
            PatchbotHelperGame game = Config.Data.PatchbotHelper.FindGame(gameName);
            if (game == null)
            {
                await SendGameNotFoundAsync(message.Channel, gameName);
                return;
            }
            if (game.RemoveSubscriber(message.User.Id))
                await Config.Data.SaveAsync();
            await message.ReplyAsync($"\u2705 You will no longer be pinged about `{game.Name}` updates.");
        }
        private async Task CmdAddID(SocketCommandContext message, Match match)
        {
            if (!(message.Channel is SocketTextChannel channel))
                return;

            // validate permissions
            SocketGuildUser user = await channel.Guild.GetGuildUser(message.User);
            if (user == null)
                return;
            if (!await ValidatePermissionsAsync(channel, user, GuildPermission.ManageGuild))
                return;

            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await message.ReplyAsync("\u274C Please specify ID of bot/webhook.");
                return;
            }
            string idString = match.Groups[1].Value.Trim();
            if (!ulong.TryParse(idString, out ulong id))
            {
                await SendIDNotValid(message.Channel, idString);
                return;
            }
            if (Config.Data.PatchbotHelper.AddPatchbotID(id))
                await Config.Data.SaveAsync();
            await message.ReplyAsync($"\u2705 {MentionUtils.MentionUser(id)} added.");
        }
        private async Task CmdRemoveID(SocketCommandContext message, Match match)
        {
            if (!(message.Channel is SocketTextChannel channel))
                return;
            // validate permissions
            SocketGuildUser user = await channel.Guild.GetGuildUser(message.User);
            if (user == null)
                return;
            if (!await ValidatePermissionsAsync(channel, user, GuildPermission.ManageGuild))
                return;

            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await message.ReplyAsync("\u274C Please specify ID of bot/webhook.");
                return;
            }
            string idString = match.Groups[1].Value.Trim();
            if (!ulong.TryParse(idString, out ulong id))
            {
                await SendIDNotValid(message.Channel, idString);
                return;
            }
            if (Config.Data.PatchbotHelper.RemovePatchbotID(id))
                await Config.Data.SaveAsync();
            await message.ReplyAsync($"\u2705 {MentionUtils.MentionUser(id)} removed.");
        }

        private async Task<bool> ValidatePermissionsAsync(SocketTextChannel channel, SocketGuildUser user, GuildPermission perms)
        {
            if (!user.GuildPermissions.Has(perms))
            {
                await channel.SendMessageAsync("\u274C Insufficient permissions.");
                return false;
            }
            return true;
        }
        private Task SendGameNotFoundAsync(ISocketMessageChannel channel, string gameName)
            => channel.SendMessageAsync($"\u274C Game `{gameName}` not found!");
        private Task SendIDNotValid(ISocketMessageChannel channel, string value)
            => channel.SendMessageAsync($"\u274C `{value}` is not a valid webhook/bot ID!");
    }
}
