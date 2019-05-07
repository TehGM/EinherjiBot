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

namespace TehGM.EinherjiBot
{
    [ProductionOnly]
    class StellarisModsHandler : HandlerBase
    { 
        public StellarisModsHandler(DiscordSocketClient client, BotConfig config) : base(client, config)
        {
            if (Config.Data.StellarisMods == null)
                Config.Data.StellarisMods = new List<StellarisModInfo>();
            CommandsStack.Add(new RegexUserCommand("^stellaris mods add(?:\\s+(.+)\\s*\\|\\s*(.+)){0,1}\\s*$", CmdAddMod));
            CommandsStack.Add(new RegexUserCommand("^stellaris mods (?:remove|del|delete)(?:\\s+(.+))?$", CmdRemoveMod));
            CommandsStack.Add(new RegexUserCommand("^stellaris mods", CmdListMods));
        }

        private async Task CmdAddMod(SocketCommandContext message, Match match)
        {
            Task CreateInvalidUseResponse()
                => message.Channel.SendMessageAsync($"Please specify both name and URL of the mod.\nProper usage of this command:\n`{GetDefaultPrefix()}stellaris mods add <name> | <url>`");
            if (message.User.Id != AuthorUser.Id)
            {
                await message.Channel.SendMessageAsync("You can't order me to do that.");
                return;
            }
            if (match.Groups.Count < 3)
            {
                await CreateInvalidUseResponse();
                return;
            }

            string name = match.Groups[1].Value.Trim();
            string url = match.Groups[2].Value.Trim();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
            {
                await CreateInvalidUseResponse();
                return;
            }
            if (url.Contains(' ', StringComparison.Ordinal))
            {
                await message.Channel.SendMessageAsync("Url can't contain any spaces.");
                return;
            }

            StellarisModInfo mod = new StellarisModInfo(name, url);
            Config.Data.StellarisMods.Add(mod);
            await Config.Data.SaveAsync();
            await message.Channel.SendMessageAsync($"Added mod:\n\n{ModToMessageString(mod)}");
        }

        private async Task CmdRemoveMod(SocketCommandContext message, Match match)
        {
            if (message.User.Id != AuthorUser.Id)
            {
                await message.Channel.SendMessageAsync("You can't order me to do that.");
                return;
            }
            if (match.Groups.Count < 2)
            {
                await message.Channel.SendMessageAsync($"Please specify numbers of mods to remove. Can be multiple numbers separated with spaces.\nTo get numbers of mods, use `{GetDefaultPrefix()}stellaris mods`");
                return;
            }

            string[] idStrings = match.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            List<string> invalidIdStrings = new List<string>(idStrings.Length);
            List<int> removalList = new List<int>(idStrings.Length);
            int removedCount = 0;

            for (int i = 0; i < idStrings.Length; i++)
            {
                if (!int.TryParse(idStrings[i], out int id))
                    invalidIdStrings.Add(idStrings[i]);
                else removalList.Add(id);
            }

            removalList.Sort();

            for (int i = removalList.Count - 1; i >= 0; i--)
            {
                int id = removalList[i];
                List<StellarisModInfo> mod = Config.Data.StellarisMods;
                if (Config.Data.StellarisMods.Count >= id)
                {
                    Config.Data.StellarisMods.RemoveAt(id - 1);
                    removedCount++;
                }
            }

            string incompatibleString = invalidIdStrings.Count != 0 ? $"\nFollowing IDs are invalid: {string.Join(", ", invalidIdStrings.Select(s => $"`{s}`"))}." : null;
            if (removedCount != 0)
            {
                await Config.Data.SaveAsync();
                await message.Channel.SendMessageAsync($"Removed {removedCount} mods.{incompatibleString}");
            }
            else
                await message.Channel.SendMessageAsync($"No mods removed.{incompatibleString}");
        }

        private Task CmdListMods(SocketCommandContext message, Match match)
        {
            if (Config.Data.StellarisMods.Count == 0)
                return message.Channel.SendMessageAsync("You did not have any mod on the list.");

            string[] listStrings = new string[Config.Data.StellarisMods.Count];
            StellarisModInfo[] orderedMods = Config.Data.StellarisMods.OrderBy(mod => mod.Name).ToArray();
            for (int i = 0; i < orderedMods.Length; i++)
                listStrings[i] = ModToListString(orderedMods[i], i + 1);
            EmbedBuilder embed = new EmbedBuilder()
                .WithDescription(string.Join('\n', listStrings))
                .WithFooter($"Currently you guys are using {listStrings.Length} mods.", Client.CurrentUser.GetAvatarUrl())
                .WithTimestamp(DateTimeOffset.Now)
                .WithAuthor("Your Stellaris mods", Client.CurrentUser.GetAvatarUrl());

            return message.Channel.SendMessageAsync(null, false, embed.Build());
        }

        public static string ModToMessageString(StellarisModInfo mod)
            => $"**{mod.Name}**\n{mod.URL}";
        public static string ModToListString(StellarisModInfo mod, int num)
            => $"{num}. **{mod.Name}** - [Click to Download]({mod.URL})";
    }
}
