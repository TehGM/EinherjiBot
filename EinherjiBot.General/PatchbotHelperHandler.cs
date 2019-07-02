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
    [ProductionOnly]
    class PatchbotHelperHandler : HandlerBase
    {
        const char _namesSeparator = '|';

        public PatchbotHelperHandler(DiscordSocketClient client, BotConfig config) : base(client, config)
        {
            CommandsStack.Add(new RegexUserCommand("^patchbot sub(?:scribe)?(?: (.+))?", CmdSubscribe));
            CommandsStack.Add(new RegexUserCommand("^patchbot unsub(?:scribe)?(?: (.+))?", CmdUnsubscribe));
            CommandsStack.Add(new RegexUserCommand("^patchbot add id(?: (\\d+))?", CmdAddID));
            CommandsStack.Add(new RegexUserCommand("^patchbot (?:remove|del|delete) id(?: (\\d+))?", CmdRemoveID));
            CommandsStack.Add(new RegexUserCommand("^patchbot add game(?: (.+))?", CmdAddGame));
            CommandsStack.Add(new RegexUserCommand("^patchbot (?:remove|del|delete) game(?: (.+))?", CmdRemoveGame));
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
                await SendNameRequiredAsync(message.Channel);
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
            await message.ReplyAsync($"{Config.DefaultConfirm} You will now get pinged about `{game.Name}` updates.");
        }
        private async Task CmdUnsubscribe(SocketCommandContext message, Match match)
        {
            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await SendNameRequiredAsync(message.Channel);
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
            await message.ReplyAsync($"{Config.DefaultConfirm} You will no longer be pinged about `{game.Name}` updates.");
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
                await message.ReplyAsync($"{Config.DefaultReject} Please specify ID of bot/webhook.");
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
            await message.ReplyAsync($"{Config.DefaultConfirm} {MentionUtils.MentionUser(id)} added.");
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
                await message.ReplyAsync($"{Config.DefaultReject} Please specify ID of bot/webhook.");
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
            await message.ReplyAsync($"{Config.DefaultConfirm} {MentionUtils.MentionUser(id)} removed.");
        }
        private async Task CmdAddGame(SocketCommandContext message, Match match)
        {
            if (message.User.Id != Config.AuthorID)
            {
                await SendInsufficientPermissionsAsync(message.Channel);
                return;
            }
            // get names
            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await SendNameAndAliasesRequiredAsync(message.Channel);
                return;
            }
            string[] names = match.Groups[1].Value.Split(_namesSeparator, StringSplitOptions.RemoveEmptyEntries)
                .Select(name => name.Trim()).Where(name => !string.IsNullOrWhiteSpace(name)).ToArray();
            if (names.Length == 0)
            {
                await SendNameAndAliasesRequiredAsync(message.Channel);
                return;
            }

            // check if game doesn't yet exist
            PatchbotHelperGame game = Config.Data.PatchbotHelper.FindGame(names[0]);
            if (game == null)
            {
                game = new PatchbotHelperGame(names[0], names.TakeLast(names.Length - 1));
                Config.Data.PatchbotHelper.Games.Add(game);
            }
            // if it does, just add new aliases
            else
            {
                for (int i = 1; i < names.Length; i++)
                {
                    if (game.Aliases.Contains(names[i], StringComparer.OrdinalIgnoreCase))
                        continue;
                    game.Aliases.Add(names[i]);
                }
            }

            await Config.Data.SaveAsync();
            await message.ReplyAsync($"{Config.DefaultConfirm} Game `{game.Name}` updated.");
        }
        private async Task CmdRemoveGame(SocketCommandContext message, Match match)
        {
            if (message.User.Id != Config.AuthorID)
            {
                await SendInsufficientPermissionsAsync(message.Channel);
                return;
            }
            if (match.Groups.Count < 2 || match.Groups[1]?.Length < 1)
            {
                await SendNameRequiredAsync(message.Channel);
                return;
            }


            // check if game exists
            string gameName = match.Groups[1].Value.Trim();
            PatchbotHelperGame game = Config.Data.PatchbotHelper.FindGame(gameName);
            if (game == null)
            {
                await SendGameNotFoundAsync(message.Channel, gameName);
                return;
            }

            Config.Data.PatchbotHelper.Games.Remove(game);
            await Config.Data.SaveAsync();
            await message.ReplyAsync($"{Config.DefaultConfirm} Game `{game.Name}` removed.");
        }

        private async Task<bool> ValidatePermissionsAsync(SocketTextChannel channel, SocketGuildUser user, GuildPermission perms)
        {
            if (!user.GuildPermissions.Has(perms))
            {
                await SendInsufficientPermissionsAsync(channel);
                return false;
            }
            return true;
        }

        private Task SendInsufficientPermissionsAsync(ISocketMessageChannel channel)
            => channel.SendMessageAsync($"{Config.DefaultReject} Insufficient permissions.");
        private Task SendGameNotFoundAsync(ISocketMessageChannel channel, string gameName)
            => channel.SendMessageAsync($"{Config.DefaultReject} Game `{gameName}` not found!");
        private Task SendIDNotValid(ISocketMessageChannel channel, string value)
            => channel.SendMessageAsync($"{Config.DefaultReject} `{value}` is not a valid webhook/bot ID!");
        private Task SendNameAndAliasesRequiredAsync(ISocketMessageChannel channel)
            => channel.SendMessageAsync($"{Config.DefaultReject} Please specify game name and aliases (separated with `{_namesSeparator}`).");
        private Task SendNameRequiredAsync(ISocketMessageChannel channel)
            => channel.SendMessageAsync($"{Config.DefaultReject} Please specify game name.");
    }
}
