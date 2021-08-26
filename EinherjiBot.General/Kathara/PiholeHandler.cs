using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TehGM.EinherjiBot.CommandsProcessing;

namespace TehGM.EinherjiBot.Kathara
{
    [RegexCommandsModule]
    [HelpCategory("Special", -99999)]
    public class PiholeHandler
    {
        private readonly ILogger _log;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<PiholeOptions> _piholeOptions;
        private readonly IOptionsMonitor<EinherjiOptions> _einherjiOptions;
        private readonly IOptionsMonitor<CommandsOptions> _commandsOptions;

        public PiholeHandler(ILogger<PiholeHandler> log, IHttpClientFactory httpClientFactory,
            IOptionsMonitor<PiholeOptions> piholeOptions, IOptionsMonitor<EinherjiOptions> einherjiOptions, IOptionsMonitor<CommandsOptions> commandsOptions)
        {
            this._log = log;
            this._httpClientFactory = httpClientFactory;
            this._einherjiOptions = einherjiOptions;
            this._piholeOptions = piholeOptions;
            this._commandsOptions = commandsOptions;
        }

        [RegexCommand("^pihole")]
        [Name("pihole")]
        [Summary("Access to commands for managing PiHole instances in TehGM's Kathara network.")]
        [RestrictCommand]
        [Priority(-105)]
        private Task CmdHelpAsync(CommandContext context, CancellationToken cancellationToken = default)
        {
            PiholeOptions options = _piholeOptions.CurrentValue;
            IReadOnlyDictionary<string, PiholeInstanceOptions> instances = options.GetUserAuthorizedInstances(context.User);

            // check user is authorized to manage any instance
            if (instances?.Any() != true)
                return context.ReplyAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} You are not authorized to manage any of Kathara PiHole instance.");

            string prefix = _commandsOptions.CurrentValue.Prefix;
            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithThumbnail("https://upload.wikimedia.org/wikipedia/en/thumb/1/15/Pi-hole_vector_logo.svg/1200px-Pi-hole_vector_logo.svg.png")
                .WithDescription("Management utility for PiHoles present in Kathara network");
            embed.AddField("Commands",
                $"**{prefix}pihole <instance id>** - show info on specific PiHole instance\n" +
                $"**{prefix}pihole <instance id> enable** - enable a specific PiHole instance\n" +
                $"**{prefix}pihole <instance id> disable** - disable a specific PiHole instance for {options.DefaultDisableTime.TotalMinutes} minutes\n" +
                $"**{prefix}pihole <instance id> disable <minutes>** - disable a specific PiHole instance for custom amount of minutes\n" +
                $"**{prefix}pihole** - display this message and check which instances you can manage");
            embed.AddField("Instances you can manage", string.Join(", ", instances.Keys));

            return context.ReplyAsync(null, embed.Build());
        }

        [RegexCommand(@"^pihole\s+(\S{1,})")]
        [Hidden]
        [RestrictCommand]
        [Priority(-103)]
        private async Task CmdInstanceInfoAsync(CommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            PiholeOptions piholeOptions = _piholeOptions.CurrentValue;
            EinherjiOptions einherjiOptions = _einherjiOptions.CurrentValue;
            string instanceID = match.Groups[1].Value;

            // check instance exists
            if (!piholeOptions.Instances.TryGetValue(instanceID, out PiholeInstanceOptions instance))
            {
                await context.ReplyAsync($"{einherjiOptions.FailureSymbol} Unknown PiHole instance `{instanceID}`.").ConfigureAwait(false);
                return;
            }
            string instanceName = string.IsNullOrWhiteSpace(instance.DisplayName) ? instanceID : instance.DisplayName;
            // check user is authorized to manage that instance
            if (!await ValidateInstanceAuthorizationAsync(context, instanceID, instance).ConfigureAwait(false))
                return;

            // send notification to user that we're working on it
            DiscordMessage workingNotification = await context.ReplyAsync("Querying pihole API, please wait...").ConfigureAwait(false);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithThumbnail("https://upload.wikimedia.org/wikipedia/en/thumb/1/15/Pi-hole_vector_logo.svg/1200px-Pi-hole_vector_logo.svg.png")
                .WithDescription($"Information on **{instanceName}** Kathara PiHole instance");

            // add stored instance config
            if (!instance.HideURL)
                embed.AddField("PiHole Address", instance.PiholeURL, false);
            if (instance.AuthorizedUserIDs.Count > 0)
                embed.AddField("Authorized users", string.Join(", ", instance.AuthorizedUserIDs.Select(uid => MentionID.User(uid))), true);
            if (instance.AuthorizedRoleIDs.Count > 0)
                embed.AddField("Authorized roles", string.Join(", ", instance.AuthorizedRoleIDs.Select(rid => MentionID.Role(rid))), true);
            if (instance.AuthorizedUserIDs.Count + instance.AuthorizedRoleIDs.Count == 0)
                embed.AddField("Authorized users", "-", true);

            // communicate with pihole API
            _log.LogDebug("Requesting status from Pihole API at {URL} (Instance: {Instance})", instance.PiholeURL, instanceName);
            HttpClient client = _httpClientFactory.CreateClient();
            using (HttpResponseMessage response = await client.GetAsync($"{instance.PiholeURL}/admin/api.php?summary", cancellationToken).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    embed.WithColor(einherjiOptions.EmbedErrorColor);
                    await context.ReplyAsync($"{einherjiOptions.FailureSymbol} Request to PiHole API failed: {response.ReasonPhrase} ({response.StatusCode})", embed.Build()).ConfigureAwait(false);
                    await workingNotification.DeleteAsync().ConfigureAwait(false);
                    return;
                }
                string responseContentRaw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!TryParseJObject(responseContentRaw, out JObject responseContentJson))
                {
                    embed.WithColor(einherjiOptions.EmbedErrorColor);
                    await context.ReplyAsync($"{einherjiOptions.FailureSymbol} Failed to query PiHole. Please refer to bot logs.", embed.Build()).ConfigureAwait(false);
                    await workingNotification.DeleteAsync().ConfigureAwait(false);
                    _log.LogError("Failed querying PiHole instance {InstanceID}: {ResponseMessage}", instanceName, responseContentRaw);
                    return;
                }

                // deserialize response
                JsonSerializer serializer = JsonSerializer.CreateDefault();
                serializer.Culture = CultureInfo.InvariantCulture;
                PiholeApiStatusResponse data = responseContentJson.ToObject<PiholeApiStatusResponse>(serializer);

                // put into embed
                embed.AddField("Status", data.IsEnabled ? $"{einherjiOptions.SuccessSymbol} Enabled" : $"{einherjiOptions.FailureSymbol} Disabled", false);
                embed.WithColor(data.IsEnabled ? einherjiOptions.EmbedSuccessColor : einherjiOptions.EmbedErrorColor);
                embed.AddField("Domains on block list", data.DomainsBeingBlocked.ToString("N0"), true);
                embed.AddField("DNS queries today", data.DnsQueriesToday.ToString("N0"), true);
                embed.AddField("Ads blocked today", $"{data.AdsBlockedToday:N0} ({data.AdsPercentageToday}%)", true);
                embed.AddField("Unique clients", $"{data.UniqueRecentClients:N0} recently, {data.ClientsEverSeen:N0} all-time", true);
                embed.WithFooter("Gravity last updated");
                embed.WithTimestamp(data.GravityLastUpdated);
            }
            using (HttpResponseMessage response = await client.GetAsync($"{instance.PiholeURL}/admin/api.php?version"))
            {
                if (response.IsSuccessStatusCode &&
                    TryParseJObject(await response.Content.ReadAsStringAsync(), out JObject responseContentJson) &&
                    !string.IsNullOrWhiteSpace(responseContentJson["version"]?.ToString()))
                    embed.AddField("PiHole version", responseContentJson["version"].ToString());
            }

            await context.ReplyAsync(null, embed.Build()).ConfigureAwait(false);
            await workingNotification.DeleteAsync().ConfigureAwait(false);
        }

        [RegexCommand(@"^pihole\s+(\S{1,})\s+enable")]
        [Hidden]
        [RestrictCommand]
        [Priority(-102)]
        private async Task CmdEnableAsync(CommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            PiholeOptions piholeOptions = _piholeOptions.CurrentValue;
            EinherjiOptions einherjiOptions = _einherjiOptions.CurrentValue;
            string instanceID = match.Groups[1].Value;

            // check instance exists
            if (!piholeOptions.Instances.TryGetValue(instanceID, out PiholeInstanceOptions instance))
            {
                await context.ReplyAsync($"{einherjiOptions.FailureSymbol} Unknown PiHole instance `{instanceID}`.").ConfigureAwait(false);
                return;
            }
            string instanceName = string.IsNullOrWhiteSpace(instance.DisplayName) ? instanceID : instance.DisplayName;
            // check user is authorized to manage that instance
            if (!await ValidateInstanceAuthorizationAsync(context, instanceID, instance).ConfigureAwait(false))
                return;

            // send notification to user that we're working on it
            DiscordMessage workingNotification = await context.ReplyAsync("Querying pihole API, please wait...").ConfigureAwait(false);

            // communicate with pihole API
            _log.LogDebug("Enabling Pihole through API at {URL} (Instance: {Instance})", instance.PiholeURL, instanceName);
            HttpClient client = _httpClientFactory.CreateClient();
            using HttpResponseMessage response = await client.GetAsync($"{instance.PiholeURL}/admin/api.php?enable&auth={instance.AuthToken}", cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                await context.ReplyAsync($"{einherjiOptions.FailureSymbol} Request to PiHole API failed: {response.ReasonPhrase} ({response.StatusCode})").ConfigureAwait(false);
                await workingNotification.DeleteAsync().ConfigureAwait(false);
                return;
            }
            string responseContentRaw = await response.Content.ReadAsStringAsync();
            if (!TryParseJObject(responseContentRaw, out JObject responseContentJson))
            {
                await context.ReplyAsync($"{einherjiOptions.FailureSymbol} Failed to enable PiHole. Please refer to bot logs.").ConfigureAwait(false);
                await workingNotification.DeleteAsync().ConfigureAwait(false);
                _log.LogError("Failed enabling PiHole instance {InstanceID}: {ResponseMessage}", instanceName, responseContentRaw);
                return;
            }
            if (!string.Equals(responseContentJson["status"]?.ToString(), "enabled", StringComparison.OrdinalIgnoreCase))
            {
                await context.ReplyAsync($"{einherjiOptions.FailureSymbol} Failed to enable PiHole. Please refer to bot logs.").ConfigureAwait(false);
                await workingNotification.DeleteAsync().ConfigureAwait(false);
                _log.LogError("Failed enabling PiHole instance {InstanceID}: 'status' is not 'enabled'", instanceName);
                return;
            }

            await context.ReplyAsync($"{einherjiOptions.SuccessSymbol} PiHole instance `{instanceName}` has been enabled.").ConfigureAwait(false);
            await workingNotification.DeleteAsync().ConfigureAwait(false);
        }

        [RegexCommand(@"^pihole\s+(\S{1,})\s+disable(?:\s+(\d+))?")]
        [Hidden]
        [RestrictCommand]
        [Priority(-101)]
        private async Task CmdDisableAsync(CommandContext context, Match match, CancellationToken cancellationToken = default)
        {
            PiholeOptions piholeOptions = _piholeOptions.CurrentValue;
            EinherjiOptions einherjiOptions = _einherjiOptions.CurrentValue;
            string instanceID = match.Groups[1].Value;

            // check instance exists
            if (!piholeOptions.Instances.TryGetValue(instanceID, out PiholeInstanceOptions instance))
            {
                await context.ReplyAsync($"{einherjiOptions.FailureSymbol} Unknown PiHole instance `{instanceID}`.").ConfigureAwait(false);
                return;
            }
            string instanceName = string.IsNullOrWhiteSpace(instance.DisplayName) ? instanceID : instance.DisplayName;


            // check user is authorized to access that instance
            if (!await ValidateInstanceAuthorizationAsync(context, instanceID, instance).ConfigureAwait(false))
                return;

            // parse disable time, defaulting to 5 mins
            TimeSpan disableTime = piholeOptions.DefaultDisableTime;
            if (uint.TryParse(match.Groups[2]?.Value, out uint disableMinutes))
                disableTime = TimeSpan.FromMinutes(disableMinutes);
            if (disableTime <= TimeSpan.FromMinutes(1))
            {
                await context.ReplyAsync($"{einherjiOptions.FailureSymbol} Minimum disable time is 1 minute.").ConfigureAwait(false);
                return;
            }

            // send notification to user that we're working on it
            DiscordMessage workingNotification = await context.ReplyAsync("Querying pihole API, please wait...").ConfigureAwait(false);

            // communicate with pihole API
            _log.LogDebug("Disabling Pihole through API at {URL} for {Duration} (Instance: {Instance})", instance.PiholeURL, instanceName, disableTime);
            HttpClient client = _httpClientFactory.CreateClient();
            using HttpResponseMessage response =
                await client.GetAsync($"{instance.PiholeURL}/admin/api.php?disable={disableMinutes * 60}&auth={instance.AuthToken}", cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                await context.ReplyAsync($"{einherjiOptions.FailureSymbol} Request to PiHole API failed: {response.ReasonPhrase} ({response.StatusCode})").ConfigureAwait(false);
                await workingNotification.DeleteAsync().ConfigureAwait(false);
                return;
            }
            string responseContentRaw = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!TryParseJObject(responseContentRaw, out JObject responseContentJson))
            {
                await context.ReplyAsync($"{einherjiOptions.FailureSymbol} Failed to disable PiHole. Please refer to bot logs.").ConfigureAwait(false);
                await workingNotification.DeleteAsync().ConfigureAwait(false);
                _log.LogError("Failed disabling PiHole instance {InstanceID}: {ResponseMessage}", instanceName, responseContentRaw);
                return;
            }
            if (!string.Equals(responseContentJson["status"]?.ToString(), "disabled", StringComparison.OrdinalIgnoreCase))
            {
                await context.ReplyAsync($"{einherjiOptions.FailureSymbol} Failed to disable PiHole. Please refer to bot logs.").ConfigureAwait(false);
                await workingNotification.DeleteAsync().ConfigureAwait(false);
                _log.LogError("Failed disabling PiHole instance {InstanceID}: 'status' is not 'disabled'", instanceName);
                return;
            }

            await context.ReplyAsync($"{einherjiOptions.FailureSymbol} PiHole instance `{instanceName}` has been disabled for {disableTime.TotalMinutes} minutes.").ConfigureAwait(false);
            await workingNotification.DeleteAsync().ConfigureAwait(false);
        }

        private string GetInstanceName(string instanceID, PiholeInstanceOptions instance)
            => string.IsNullOrWhiteSpace(instance.DisplayName) ? instanceID : instance.DisplayName;

        private async Task<bool> ValidateInstanceAuthorizationAsync(CommandContext context, string instanceID, PiholeInstanceOptions instance)
        {
            DiscordUser user = (await context.GetGuildMemberAsync().ConfigureAwait(false)) ?? context.User;
            if (!instance.IsAuthorized(user))
            {
                await context.ReplyAsync($"{_einherjiOptions.CurrentValue.FailureSymbol} You have no permissions to access PiHole instance `{GetInstanceName(instanceID, instance)}`.").ConfigureAwait(false);
                return false;
            }
            return true;
        }

        private static bool TryParseJObject(string content, out JObject json)
        {
            try
            {
                json = JObject.Parse(content);
                return true;
            }
            catch
            {
                json = null;
                return false;
            }
        }
    }
}
