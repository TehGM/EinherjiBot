using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing;
using Discord.Commands;
using System.Text.RegularExpressions;
using System.Threading;
using TehGM.EinherjiBot.EliteDangerous;
using TehGM.EinherjiBot.CommandsProcessing.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TehGM.EinherjiBot
{
    [LoadRegexCommands]
    public class HelpHandler
    {
        private readonly CommandsOptions _commandsOptions;
        private readonly EinherjiOptions _einherjiOptions;
        private readonly CommunityGoalsOptions _eliteOptions;
        private readonly ILogger _log;
        private readonly SimpleCommandHandler _simpleCommands;
        private readonly RegexCommandHandler _regexCommands;

        public HelpHandler(ILogger<HelpHandler> log, IOptionsSnapshot<EinherjiOptions> einherjiOptions, IOptionsSnapshot<CommandsOptions> commandsOptions,
            IOptionsSnapshot<CommunityGoalsOptions> eliteOptions, SimpleCommandHandler simpleCommands, RegexCommandHandler regexCommands)
        {
            this._commandsOptions = commandsOptions.Value;
            this._einherjiOptions = einherjiOptions.Value;
            this._eliteOptions = eliteOptions.Value;
            this._log = log;
            this._simpleCommands = simpleCommands;
            this._regexCommands = regexCommands;
        }

        [RegexCommand(@"^help")]
        [Hidden]
        [Priority(-999999)]
        private Task CmdGetAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.Title = $"{context.Client.CurrentUser.Username} Bot";
            embed.Description = $"Personal administration bot developed by {GetAuthorText(context)}.";
            embed.ThumbnailUrl = context.Client.CurrentUser.GetMaxAvatarUrl();

            IOrderedEnumerable<IGrouping<string, CommandDescriptor>> commands = this.GetCommandDescriptors(context);
            if (commands.Any())
            {
                string prefix = _commandsOptions.Prefix;
                if (string.IsNullOrWhiteSpace(prefix))
                    prefix = $"{MentionUtils.MentionUser(context.Client.CurrentUser.Id)} ";

                StringBuilder commandsList = new StringBuilder();
                foreach (IGrouping<string, CommandDescriptor> group in commands)
                {
                    commandsList.AppendFormat("__{0}__\n", group.Key);
                    foreach (CommandDescriptor cmd in group)
                        commandsList.Append($"***{prefix}{cmd.DisplayName}***: {cmd.Summary}\n");
                }
                embed.AddField("Commands", commandsList.ToString(), inline: false);
            }

            if (this.IsMainRestrictionGroup(context))
            {
                embed.AddField("Additional features",
                    "If you try to use an another bot in a wrong channel, I'll direct you to the correct channel.\n" +
                    $"I'll automatically post new or just finished Elite Dangerous Community Goals in {MentionUtils.MentionChannel(_eliteOptions.AutoNewsChannelID)}.\n" +
                    $"I'll post a message in {GetLeaveChannel(context)} when a user leaves the guild.",
                    inline: false);
            }
            else
            {
                embed.AddField("Additional features", 
                    $"I'll post a message in {GetLeaveChannel(context)} when a user leaves the guild.\n" + 
                    $"More additional features are provided in {GetAuthorText(context)}'s server.", inline: false);
            }
            embed.AddField("Support",
                $"To submit bugs or suggestions, please open an issue on [GitHub](https://github.com/TehGM/EinherjiBot/issues). Alternatively, you can message {GetAuthorText(context)}.\n" +
                "To support the developer, consider donating on [GitHub Sponsors](https://github.com/sponsors/TehGM), [Patreon](https://patreon.com/TehGMdev) or [Buy Me A Coffee](https://www.buymeacoffee.com/TehGM). **Thank you!**",
                inline: false);
            embed.WithFooter($"{context.Client.CurrentUser.Username} Bot, v{BotInfoUtility.GetVersion()}", context.Client.CurrentUser.GetSafeAvatarUrl());

            return context.ReplyAsync(null, false, embed.Build(), cancellationToken); 
        }

        private bool IsMainRestrictionGroup(ICommandContext context)
        {
            if (context.Guild == null)
                return false;
            if (!this._commandsOptions.RestrictionGroups.TryGetValue(CommandRestrictionGroup.MainGuild, out CommandRestrictionGroup group))
                return false;
            return group.GuildIDs.Contains(context.Guild.Id);
        }

        private IOrderedEnumerable<IGrouping<string, CommandDescriptor>> GetCommandDescriptors(ICommandContext context)
        {
            // get commands
            IEnumerable<CommandDescriptor> commands = new List<CommandDescriptor>();
            commands = commands.Union(this._regexCommands.Commands.Select(cmd => new CommandDescriptor(cmd)));
            commands = commands.Union(this._simpleCommands.Commands.Commands.Select(cmd => new CommandDescriptor(cmd)));

            // exclude hidden, unnamed, without summary, and ones that are restricted
            commands = commands.Where(cmd =>
                !cmd.IsHidden &&
                !string.IsNullOrWhiteSpace(cmd.DisplayName) &&
                !string.IsNullOrWhiteSpace(cmd.Summary) &&
                (cmd.Restrictions == null || cmd.Restrictions.CheckRestriction(context, this._commandsOptions.RestrictionGroups)));

            // order commands based on priority and name
            IOrderedEnumerable<CommandDescriptor> orderedCommands = commands
                .OrderByDescending(cmd => cmd.Priority)
                .ThenBy(cmd => cmd.DisplayName);

            // group commands by category
            IEnumerable<IGrouping<string, CommandDescriptor>> groups = orderedCommands.GroupBy(cmd => cmd.HelpCategory?.CategoryName);

            // exclude empty groups
            groups = groups.Where(grp => grp.Any());

            // order groups
            IOrderedEnumerable<IGrouping<string, CommandDescriptor>> orderedGroups = groups
                .OrderByDescending(grp => grp.FirstOrDefault()?.HelpCategory?.Priority)
                .ThenBy(grp => grp.FirstOrDefault()?.HelpCategory?.CategoryName);

            return orderedGroups;
        }

        private string BuildCommandLine(string command, string description, SocketCommandContext context)
        {
            string prefix = _commandsOptions.Prefix;
            if (string.IsNullOrWhiteSpace(prefix))
                prefix = $"{MentionUtils.MentionUser(context.Client.CurrentUser.Id)} ";
            return $"***{prefix}{command}***: {description}\n";
        }

        private string GetLeaveChannel(SocketCommandContext context)
        {
            if (context.IsPrivate)
                return "a guild channel";
            else
                return MentionUtils.MentionChannel(context.Guild.SystemChannel.Id);
        }

        private string GetAuthorText(SocketCommandContext context)
        {
            ulong id = _einherjiOptions.AuthorID;
            if (!context.IsPrivate)
            {
                SocketGuildUser guildUser = context.Guild.GetUser(id);
                if (guildUser != null)
                    return MentionUtils.MentionUser(id);
            }
            SocketUser user = context.Client.GetUser(id);
            if (user != null)
                return $"{user.Username}#{user.Discriminator}";
            return "TehGM";
        }
    }
}
