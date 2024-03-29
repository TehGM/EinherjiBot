﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing;

namespace TehGM.EinherjiBot.Stellaris.Services
{
    [LoadRegexCommands]
    [HelpCategory("Games", 10)]
    public class StellarisModsHandler
    {
        private readonly IStellarisModsStore _stellarisModsStore;
        private readonly EinherjiOptions _einherjiOptions;
        private readonly CommandsOptions _commandsOptions;
        private readonly ILogger _log;

        public StellarisModsHandler(IStellarisModsStore stellarisModsStore, ILogger<StellarisModsHandler> log, IOptionsSnapshot<EinherjiOptions> einherjiOptions, IOptionsSnapshot<CommandsOptions> commandsOptions)
        {
            this._stellarisModsStore = stellarisModsStore;
            this._log = log;
            this._einherjiOptions = einherjiOptions.Value;
            this._commandsOptions = commandsOptions.Value;
        }

        [RegexCommand("^stellaris mods add(?:\\s+(.+)\\s*\\|\\s*(.+)){0,1}\\s*$")]
        [Hidden]
        [Priority(400)]
        private async Task CmdAddModAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            Task CreateInvalidUseResponse()
                => context.ReplyAsync($"{_einherjiOptions.FailureSymbol} Please specify both name and URL of the mod.\nProper usage of this command:\n***{_commandsOptions.Prefix}stellaris mods add <name> | <url>***", cancellationToken);
            if (context.User.Id != _einherjiOptions.AuthorID)
            {
                await context.ReplyAsync($"{_einherjiOptions.FailureSymbol} You can't order me to do that.", cancellationToken).ConfigureAwait(false);
                return;
            }
            if (match.Groups.Count < 3)
            {
                await CreateInvalidUseResponse().ConfigureAwait(false);
                return;
            }

            string name = match.Groups[1].Value.Trim();
            string url = match.Groups[2].Value.Trim();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(url))
            {
                await CreateInvalidUseResponse().ConfigureAwait(false);
                return;
            }
            if (url.Contains(' ', StringComparison.Ordinal))
            {
                await context.ReplyAsync($"{_einherjiOptions.FailureSymbol} URL can't contain any spaces.", cancellationToken).ConfigureAwait(false);
                return;
            }

            StellarisMod mod = new StellarisMod(name, url);
            _log.LogDebug("Adding stellaris mod {ModName}", mod.Name);
            await _stellarisModsStore.AddAsync(mod, cancellationToken).ConfigureAwait(false);
            await context.ReplyAsync($"{_einherjiOptions.SuccessSymbol} Added mod:\n\n{ModToMessageString(mod)}", cancellationToken).ConfigureAwait(false);
        }

        [RegexCommand("^stellaris mods (?:remove|del|delete)(?:\\s+(.+))?$")]
        [Hidden]
        [Priority(399)]
        private async Task CmdRemoveModAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            if (context.User.Id != _einherjiOptions.AuthorID)
            {
                await context.ReplyAsync($"{_einherjiOptions.FailureSymbol} You can't order me to do that.", cancellationToken).ConfigureAwait(false);
                return;
            }
            if (match.Groups.Count < 2)
            {
                await context.ReplyAsync($"{_einherjiOptions.FailureSymbol} Please specify numbers of mods to remove. Can be multiple numbers separated with spaces.\nTo get numbers of mods, use `{_commandsOptions.Prefix}stellaris mods`", cancellationToken).ConfigureAwait(false);
                return;
            }

            IOrderedEnumerable<StellarisMod> mods = (await _stellarisModsStore.GetAllAsync(cancellationToken).ConfigureAwait(false)).OrderBy(m => m.Name);

            string[] idStrings = match.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            List<string> invalidIdStrings = new List<string>(idStrings.Length);
            List<StellarisMod> removalList = new List<StellarisMod>(idStrings.Length);
            for (int i = 0; i < idStrings.Length; i++)
            {
                if (!int.TryParse(idStrings[i], out int id))
                    invalidIdStrings.Add(idStrings[i]);
                else
                    removalList.Add(mods.ElementAt(id));
            }

            string incompatibleString = invalidIdStrings.Count != 0 ? $"\nFollowing IDs are invalid: {string.Join(", ", invalidIdStrings.Select(s => $"`{s}`"))}." : null;
            if (removalList.Any())
            {
                _log.LogDebug("Removing {Count} stellaris mods: {ModNames}", removalList.Count, string.Join(", ", removalList.Select(s => s.Name)));
                await _stellarisModsStore.RemoveAsync(removalList, cancellationToken).ConfigureAwait(false);
                await context.ReplyAsync($"{_einherjiOptions.SuccessSymbol} Removed {removalList.Count} mods.{incompatibleString}", cancellationToken).ConfigureAwait(false);
            }
            else
                await context.ReplyAsync($"No mods removed.{incompatibleString}", cancellationToken).ConfigureAwait(false);
        }

        [RegexCommand("^stellaris mods")]
        [Name("stellaris mods")]
        [Summary("Shows list of Stellaris mods we use in multiplayer.")]
        [Priority(398)]
        [RestrictCommand]
        private async Task CmdListModsAsync(SocketCommandContext context, CancellationToken cancellationToken = default)
        {
            using IDisposable logScope = _log.BeginCommandScope(context, this);
            IOrderedEnumerable<StellarisMod> mods = (await _stellarisModsStore.GetAllAsync(cancellationToken).ConfigureAwait(false)).OrderBy(m => m.Name);
            if (!mods.Any())
            {
                await context.ReplyAsync($"{_einherjiOptions.FailureSymbol} You did not have any mod on the list.", cancellationToken).ConfigureAwait(false);
                return;
            }

            List<string> listStrings = new List<string>(mods.Count());
            foreach (StellarisMod mod in mods)
                listStrings.Add(ModToListString(mod, listStrings.Count + 1));
            EmbedBuilder embed = new EmbedBuilder()
                .WithDescription(string.Join('\n', listStrings))
                .WithFooter($"Currently you guys are using {listStrings.Count} mods.", context.Client.CurrentUser.GetAvatarUrl())
                .WithTimestamp(DateTimeOffset.Now)
                .WithAuthor("Your Stellaris mods", context.Client.CurrentUser.GetAvatarUrl());

            await context.ReplyAsync(null, false, embed.Build(), cancellationToken).ConfigureAwait(false);
        }

        private static string ModToMessageString(StellarisMod mod)
            => $"**{mod.Name}**\n{mod.URL}";
        private static string ModToListString(StellarisMod mod, int num)
            => $"{num}. **{mod.Name}** - [Click to Download]({mod.URL})";
    }
}
