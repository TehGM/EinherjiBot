using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TehGM.EinherjiBot.CommandsProcessing;
using TehGM.EinherjiBot.Config;
using TehGM.EinherjiBot.Extensions;
using TehGM.EinherjiBot.Utilities;

namespace TehGM.EinherjiBot
{
    [ProductionOnly]
    public class PiholeHandler : HandlerBase
    {
        public PiholeHandler(DiscordSocketClient client, BotConfig config) : base(client, config)
        {
            CommandsStack.Add(new RegexUserCommand(new Regex(@"^pihole\s+(\S{1,})\s+disable(?:\s+(\d+))?", RegexUserCommand.DefaultRegexOptions), CmdDisable));
            CommandsStack.Add(new RegexUserCommand(new Regex(@"^pihole\s+(\S{1,})\s+enable", RegexUserCommand.DefaultRegexOptions), CmdEnable));
            CommandsStack.Add(new RegexUserCommand(new Regex(@"^pihole\s+(\S{1,})", RegexUserCommand.DefaultRegexOptions), CmdInstanceInfo));
            CommandsStack.Add(new RegexUserCommand(new Regex(@"^pihole", RegexUserCommand.DefaultRegexOptions), CmdHelp));
        }

        private Task CmdHelp(SocketCommandContext message, Match match)
        {
            PiholeConfig piholeConfig = Config.Auth.Pihole;
            IEnumerable<PiholeInstanceConfig> instances = piholeConfig.UserAuthorizedInstances(message.User);

            // check user is authorized to manage any instance
            if (!instances.Any())
                return message.ReplyAsync($"{Config.DefaultReject} You are not authorized to manage any of Kathara PiHole instance.");

            EmbedBuilder embed = new EmbedBuilder()
                .WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/en/thumb/1/15/Pi-hole_vector_logo.svg/1200px-Pi-hole_vector_logo.svg.png")
                .WithDescription("Management utility for PiHoles present in Kathara network");
            embed.AddField("Commands",
                $"**{GetDefaultPrefix()}pihole <instance id>** - show info on specific PiHole instance\n" +
                $"**{GetDefaultPrefix()}pihole <instance id> enable** - enable a specific PiHole instance\n" +
                $"**{GetDefaultPrefix()}pihole <instance id> disable** - disable a specific PiHole instance for {piholeConfig.DefaultDisableMinutes} minutes\n" +
                $"**{GetDefaultPrefix()}pihole <instance id> disable <minutes>** - disable a specific PiHole instance for custom amount of minutes\n" +
                $"**{GetDefaultPrefix()}pihole** - display this message and check which instances you can manage");
            embed.AddField("Instances you can manage", string.Join(", ", instances.Select(inst => $"`{inst.InstanceID}`")));

            return message.ReplyAsync(null, false, embed.Build());
        }

        private async Task CmdInstanceInfo(SocketCommandContext message, Match match)
        {
            PiholeConfig piholeConfig = Config.Auth.Pihole;
            string instanceID = match.Groups[1].Value;

            // check instance exists
            if (!piholeConfig.Instances.TryGetValue(instanceID, out PiholeInstanceConfig instance))
            {
                await message.ReplyAsync($"{Config.DefaultReject} Unknown PiHole instance `{instanceID}`.");
                return;
            }
            // check user is authorized to manage that instance
            if (!instance.IsAuthorized(message.User))
            {
                await message.ReplyAsync($"{Config.DefaultReject} You have no permissions to manage PiHole instance `{instanceID}`.");
                return;
            }

            // send notification to user that we're working on it
            RestUserMessage workingNotification = await message.ReplyAsync("Querying pihole API, please wait...");

            EmbedBuilder embed = new EmbedBuilder()
                .WithThumbnailUrl("https://upload.wikimedia.org/wikipedia/en/thumb/1/15/Pi-hole_vector_logo.svg/1200px-Pi-hole_vector_logo.svg.png")
                .WithDescription($"Information on **{instance.InstanceID}** Kathara PiHole instance");

            // add stored instance config
            if (!instance.HideURL)
                embed.AddField("PiHole Address", instance.PiholeURL, false);
            if (instance.AuthorizedUsersIDs.Count > 0)
                embed.AddField("Authorized users", string.Join(", ", instance.AuthorizedUsersIDs.Select(uid => MentionUtils.MentionUser(uid))), true);
            if (instance.AuthorizedRolesIDs.Count > 0)
                embed.AddField("Authorized roles", string.Join(", ", instance.AuthorizedRolesIDs.Select(rid => MentionUtils.MentionRole(rid))), true);
            if (instance.AuthorizedUsersIDs.Count + instance.AuthorizedRolesIDs.Count == 0)
                embed.AddField("Authorized users", "-", true);

            // communicate with pihole API
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync($"{instance.PiholeURL}/admin/api.php?summary"))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        embed.WithColor(Color.DarkRed);
                        await message.ReplyAsync($"{Config.DefaultReject} Request to PiHole API failed: {response.ReasonPhrase} ({response.StatusCode})", false, embed.Build());
                        await workingNotification.DeleteAsync();
                        return;
                    }
                    string responseContentRaw = await response.Content.ReadAsStringAsync();
                    if (!TryParseJObject(responseContentRaw, out JObject responseContentJson))
                    {
                        embed.WithColor(Color.DarkRed);
                        await message.ReplyAsync($"{Config.DefaultReject} Failed to query PiHole. Please refer to bot logs.", false, embed.Build());
                        await workingNotification.DeleteAsync();
                        Logging.Default.Error("Failed querying PiHole instance {InstanceID}: {ResponseMessage}", instance.InstanceID, responseContentRaw);
                        return;
                    }

                    // gather all required data
                    bool isEnabled = responseContentJson["status"]?.ToString() == "enabled";
                    string domainsOnBlockList = responseContentJson["domains_being_blocked"]?.ToString();
                    string queriesToday = responseContentJson["dns_queries_today"]?.ToString();
                    string adsBlockedToday = responseContentJson["ads_blocked_today"]?.ToString();
                    string adsBlockedTodayPerc = responseContentJson["ads_percentage_today"]?.ToString();
                    string recentUniqueClients = responseContentJson["unique_clients"]?.ToString();
                    string allUniqueClients = responseContentJson["clients_ever_seen"]?.ToString();
                    string gravityUpdateTimestamp = responseContentJson["gravity_last_updated"]?["absolute"]?.ToString();

                    // put into embed
                    embed.AddField("Status", isEnabled ? $"{Config.DefaultConfirm} Enabled" : $"{Config.DefaultReject} Disabled", false);
                    embed.WithColor(isEnabled ? Color.Green : Color.Red);
                    if (!string.IsNullOrWhiteSpace(domainsOnBlockList))
                        embed.AddField("Domains on block list", domainsOnBlockList, true);
                    if (!string.IsNullOrWhiteSpace(queriesToday))
                        embed.AddField("DNS queries today", queriesToday, true);
                    if (!string.IsNullOrWhiteSpace(adsBlockedToday))
                    {
                        string fieldValue = adsBlockedToday;
                        if (!string.IsNullOrWhiteSpace(adsBlockedTodayPerc))
                            fieldValue += $" ({adsBlockedTodayPerc}%)";
                        embed.AddField("Ads blocked today", fieldValue, true);
                    }
                    else if (!string.IsNullOrWhiteSpace(adsBlockedTodayPerc))
                        embed.AddField("Ads blocked today", $"{adsBlockedTodayPerc}%", true);
                    if (!string.IsNullOrWhiteSpace(recentUniqueClients))
                    {
                        string fieldValue = $"{recentUniqueClients} recently";
                        if (!string.IsNullOrWhiteSpace(allUniqueClients))
                            fieldValue += $", {allUniqueClients} all-time";
                        embed.AddField("Unique clients", fieldValue, true);
                    }
                    else if (!string.IsNullOrWhiteSpace(allUniqueClients))
                        embed.AddField("Unique clients", $"{allUniqueClients} all-time", true);
                    if (gravityUpdateTimestamp != null && long.TryParse(gravityUpdateTimestamp, out long gravityUpdateEpoch))
                    {
                        embed.WithFooter("Gravity last updated");
                        embed.WithTimestamp(DateTimeOffset.UnixEpoch.AddSeconds(gravityUpdateEpoch));
                    }
                }
                using (HttpResponseMessage response = await client.GetAsync($"{instance.PiholeURL}/admin/api.php?version"))
                {
                    if (response.IsSuccessStatusCode &&
                        TryParseJObject(await response.Content.ReadAsStringAsync(), out JObject responseContentJson) &&
                        !string.IsNullOrWhiteSpace(responseContentJson["version"]?.ToString()))
                        embed.AddField("PiHole version", responseContentJson["version"].ToString());
                }
            }

            await message.ReplyAsync(null, false, embed.Build());
            await workingNotification.DeleteAsync();
        }

        private async Task CmdEnable(SocketCommandContext message, Match match)
        {
            PiholeConfig piholeConfig = Config.Auth.Pihole;
            string instanceID = match.Groups[1].Value;

            // check instance exists
            if (!piholeConfig.Instances.TryGetValue(instanceID, out PiholeInstanceConfig instance))
            {
                await message.ReplyAsync($"{Config.DefaultReject} Unknown PiHole instance `{instanceID}`.");
                return;
            }
            // check user is authorized to manage that instance
            if (!instance.IsAuthorized(message.User))
            {
                await message.ReplyAsync($"{Config.DefaultReject} You have no permissions to manage PiHole instance `{instanceID}`.");
                return;
            }

            // send notification to user that we're working on it
            RestUserMessage workingNotification = await message.ReplyAsync("Querying pihole API, please wait...");

            // communicate with pihole API
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response =
                    await client.GetAsync($"{instance.PiholeURL}/admin/api.php?enable&auth={instance.AuthToken}"))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        await message.ReplyAsync($"{Config.DefaultReject} Request to PiHole API failed: {response.ReasonPhrase} ({response.StatusCode})");
                        await workingNotification.DeleteAsync();
                        return;
                    }
                    string responseContentRaw = await response.Content.ReadAsStringAsync();
                    if (!TryParseJObject(responseContentRaw, out JObject responseContentJson))
                    {
                        await message.ReplyAsync($"{Config.DefaultReject} Failed to enable PiHole. Please refer to bot logs.");
                        await workingNotification.DeleteAsync();
                        Logging.Default.Error("Failed enabling PiHole instance {InstanceID}: {ResponseMessage}", instance.InstanceID, responseContentRaw);
                        return;
                    }
                    if (!string.Equals(responseContentJson["status"]?.ToString(), "enabled", StringComparison.OrdinalIgnoreCase))
                    {
                        await message.ReplyAsync($"{Config.DefaultReject} Failed to enable PiHole. Please refer to bot logs.");
                        await workingNotification.DeleteAsync();
                        Logging.Default.Error("Failed enabling PiHole instance {InstanceID}: 'status' is not 'enabled'", instanceID);
                        return;
                    }

                    await message.ReplyAsync($"{Config.DefaultConfirm} PiHole instance `{instance.InstanceID}` has been enabled.");
                    await workingNotification.DeleteAsync();
                }
            }
        }

        private async Task CmdDisable(SocketCommandContext message, Match match)
        {
            PiholeConfig piholeConfig = Config.Auth.Pihole;
            string instanceID = match.Groups[1].Value;

            // check instance exists
            if (!piholeConfig.Instances.TryGetValue(instanceID, out PiholeInstanceConfig instance))
            {
                await message.ReplyAsync($"{Config.DefaultReject} Unknown PiHole instance `{instanceID}`.");
                return;
            }
            // check user is authorized to access that instance
            if (!instance.IsAuthorized(message.User))
            {
                await message.ReplyAsync($"{Config.DefaultReject} You have no permissions to access PiHole instance `{instanceID}`.");
                return;
            }

            // parse disable time, defaulting to 5 mins
            if (!uint.TryParse(match.Groups[2]?.Value, out uint disableMinutes))
                disableMinutes = piholeConfig.DefaultDisableMinutes;
            if (disableMinutes <= 0)
            {
                await message.ReplyAsync($"{Config.DefaultReject} Minimum disable time is 1 minute.");
                return;
            }

            // send notification to user that we're working on it
            RestUserMessage workingNotification = await message.ReplyAsync("Querying pihole API, please wait...");

            // communicate with pihole API
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response =
                    await client.GetAsync($"{instance.PiholeURL}/admin/api.php?disable={disableMinutes * 60}&auth={instance.AuthToken}"))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        await message.ReplyAsync($"{Config.DefaultReject} Request to PiHole API failed: {response.ReasonPhrase} ({response.StatusCode})");
                        await workingNotification.DeleteAsync();
                        return;
                    }
                    string responseContentRaw = await response.Content.ReadAsStringAsync();
                    if (!TryParseJObject(responseContentRaw, out JObject responseContentJson))
                    {
                        await message.ReplyAsync($"{Config.DefaultReject} Failed to disable PiHole. Please refer to bot logs.");
                        await workingNotification.DeleteAsync();
                        Logging.Default.Error("Failed disabling PiHole instance {InstanceID}: {ResponseMessage}", instance.InstanceID, responseContentRaw);
                        return;
                    }
                    if (!string.Equals(responseContentJson["status"]?.ToString(), "disabled", StringComparison.OrdinalIgnoreCase))
                    {
                        await message.ReplyAsync($"{Config.DefaultReject} Failed to disable PiHole. Please refer to bot logs.");
                        await workingNotification.DeleteAsync();
                        Logging.Default.Error("Failed disabling PiHole instance {InstanceID}: 'status' is not 'disabled'", instanceID);
                        return;
                    }

                    await message.ReplyAsync($"{Config.DefaultConfirm} PiHole instance `{instance.InstanceID}` has been disabled for {disableMinutes} minutes.");
                    await workingNotification.DeleteAsync();
                }
            }
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
