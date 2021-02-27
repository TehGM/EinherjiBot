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

namespace TehGM.EinherjiBot
{
    [LoadRegexCommands]
    public class HelpHandler
    {
        private readonly CommandsOptions _commandsOptions;
        private readonly EinherjiOptions _einherjiOptions;
        private readonly CommunityGoalsOptions _eliteOptions;
        private readonly ILogger _log;

        public HelpHandler(ILogger<HelpHandler> log, IOptionsSnapshot<EinherjiOptions> einherjiOptions, IOptionsSnapshot<CommandsOptions> commandsOptions,
            IOptionsSnapshot<CommunityGoalsOptions> eliteOptions)
        {
            this._commandsOptions = commandsOptions.Value;
            this._einherjiOptions = einherjiOptions.Value;
            this._eliteOptions = eliteOptions.Value;
            this._log = log;
        }

        [RegexCommand(@"^help")]
        [Priority(-999999)]
        private Task CmdGetAsync(SocketCommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            EmbedBuilder embed = new EmbedBuilder();
            embed.Title = $"{context.Client.CurrentUser.Username} Bot";
            embed.Description = $"Personal administration bot developed by {GetAuthorText(context)}.";
            embed.ThumbnailUrl = context.Client.CurrentUser.GetMaxAvatarUrl();
            embed.AddField("Commands",
                "__General__\n" +
                BuildCommandLine("intel", "Shows help for intel feature.", context) +
                "__Game updates__\n" +
                BuildCommandLine("patchbot subscribe <game>", "Subscribes you for pings whenever there's a patchbot update about *<game>*.", context) +
                BuildCommandLine("patchbot unsubscribe <game>", "As above, but for cancelling your subscription.", context) +
                "__Games__\n" +
                BuildCommandLine("stellaris mods", "Shows list of Stellaris mods we use in multiplayer.", context) +
                BuildCommandLine("server <game>", "If you're authorized, will give you info how to connect to our game servers.", context) +
                BuildCommandLine("elite community goals", "Shows list of currently ongoing Community Goals in Elite Dangerous.", context) +
                "__Special__\n" +
                BuildCommandLine("netflix account", $"If you're a part of our Netflix team, will provide Netflix credentials.", context) +
                BuildCommandLine("pihole", $"Access to commands for managing PiHole instances in TehGM's Kathara network.", context) +
                BuildCommandLine("purge <number>", $"Removes last *<number>* messages. You need to have permissions to remove messages.", context) +
                BuildCommandLine("move all <from-id> <to-id>", $"Moves all users from voice channel *<from-id>* to *<to-id>*. You need to have appropiate permissions.", context),
                inline: false);
            embed.AddField("Additional features",
                "If you try to use an another bot in a wrong channel, I'll direct you to the correct channel.\n" +
                $"I'll automatically post new or just finished Elite Dangerous Community Goals in {MentionUtils.MentionChannel(_eliteOptions.AutoNewsChannelID)}.\n" +
                $"I'll post a message in {GetLeaveChannel(context)} when a user leaves the guild.", 
                inline: false);
            embed.AddField("Support",
                $"To submit bugs or suggestions, please open an issue on [GitHub](https://github.com/TehGM/EinherjiBot/issues). Alternatively, you can message {GetAuthorText(context)}.\n" +
                "To support the developer, consider donating on [GitHub Sponsors](https://github.com/sponsors/TehGM), [Patreon](https://patreon.com/TehGMdev) or [Buy Me A Coffee](https://www.buymeacoffee.com/TehGM). **Thank you!**",
                inline: false);
            embed.WithFooter($"{context.Client.CurrentUser.Username} Bot, v{BotInfoUtility.GetVersion()}", context.Client.CurrentUser.GetSafeAvatarUrl());

            return context.ReplyAsync(null, false, embed.Build(), cancellationToken); 
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
