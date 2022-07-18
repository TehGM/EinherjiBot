﻿using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using System.Runtime.CompilerServices;

namespace TehGM.EinherjiBot.DiscordClient
{
    internal class DiscordCommandsService : IHostedService, IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly DiscordOptions _options;
        private readonly InteractionService _interactions;
        private readonly IServiceProvider _services;
        private readonly ILogger _log;
        private CancellationTokenSource _cts;

        public DiscordCommandsService(DiscordSocketClient client, IServiceProvider services, ILogger<DiscordCommandsService> log, IOptions<DiscordOptions> options)
        {
            this._client = client;
            this._services = services;
            this._log = log;
            this._options = options.Value;
            this._interactions = new InteractionService(this._client, new InteractionServiceConfig()
            {
                DefaultRunMode = RunMode.Async,
                UseCompiledLambda = this._options.CompileCommands
            });

            this._client.Ready += OnClientReady;
            this._client.SlashCommandExecuted += OnSlashCommandAsync;
            this._client.UserCommandExecuted += OnUserCommandAsync;
            this._client.MessageCommandExecuted += OnMessageCommandAsync;
            this._client.ButtonExecuted += OnButtonCommandAsync;
            this._client.SelectMenuExecuted += OnMenuCommandAsync;
            this._interactions.Log += OnLog;
        }

        private async Task OnClientReady()
        {
            this._log.LogTrace("Loading all command modules");

            if (this._options.OverrideCommandsGuildID != null)
            {
                this._log.LogDebug("Registering all commands for guild {GuildID}", this._options.OverrideCommandsGuildID.Value);
                await this._interactions.AddModulesAsync(this.GetType().Assembly, this._services);
                await this._interactions.RegisterCommandsToGuildAsync(this._options.OverrideCommandsGuildID.Value).ConfigureAwait(false);
            }
            else
            {
                IEnumerable<ModuleInfo> modules = await this._interactions.AddModulesAsync(this.GetType().Assembly, this._services).ConfigureAwait(false);

                this._log.LogDebug("Registering global commands");
                IEnumerable<ModuleInfo> globalCommands = modules
                    .Where(module => !module.Attributes.Any(attr => attr is GuildCommandAttribute));
                await this._interactions.AddModulesGloballyAsync(true, modules.ToArray()).ConfigureAwait(false);

                this._log.LogDebug("Registering guild commands");
                ILookup<ulong, ModuleInfo> guildCommands = modules
                    .Except(globalCommands)
                    .SelectMany(module => (module.Attributes.First(attr => attr is GuildCommandAttribute) as GuildCommandAttribute).GuildIDs
                        .Select(id => new KeyValuePair<ulong, ModuleInfo>(id, module)))
                    .ToLookup(kvp => kvp.Key, kvp => kvp.Value);
                foreach (IGrouping<ulong, ModuleInfo> group in guildCommands)
                {
                    this._log.LogDebug("Registering commands for guild {ID}", group.Key);
                    await this._interactions.AddModulesToGuildAsync(group.Key, true, group.ToArray());
                }
            }
        }


        // currently interactions don't really differ much
        // it might change later, but for now we can delegate all event handlers to the same method
        private Task OnSlashCommandAsync(SocketSlashCommand interaction)
            => this.OnInteractionAsync(interaction);
        private  Task OnUserCommandAsync(SocketUserCommand interaction)
            => this.OnInteractionAsync(interaction);
        private  Task OnMessageCommandAsync(SocketMessageCommand interaction)
            => this.OnInteractionAsync(interaction);
        private Task OnMenuCommandAsync(SocketMessageComponent interaction)
            => this.OnInteractionAsync(interaction);
        private Task OnButtonCommandAsync(SocketMessageComponent interaction)
            => this.OnInteractionAsync(interaction);

        private async Task OnInteractionAsync(SocketInteraction interaction)
        {
            EinherjiInteractionContext ctx = new EinherjiInteractionContext(this._client, interaction, this._cts.Token);
            using IDisposable logScope = this.BeginCommandScope(ctx, null, null);
            try
            {
                await this._interactions.ExecuteCommandAsync(ctx, this._services);
            }
            catch (HttpException ex) when (ex.IsMissingPermissions())
            {
                try
                {
                    await interaction.RespondAsync(
                        text: $"Bot missing permissions. Please contact guild admin.",
                        ephemeral: true,
                        options: ctx.CancellationToken.ToRequestOptions())
                        .ConfigureAwait(false);
                }
                catch { }
                throw;
            }
        }

        public IDisposable BeginCommandScope(IInteractionContext context, Type handlerType = null, [CallerMemberName] string cmdName = null)
        {
            Dictionary<string, object> state = new Dictionary<string, object>
            {
                { "Command.UserID", context.User?.Id },
                { "Command.InteractionID", context.Interaction?.Id },
                { "Command.ChannelID", context.Channel?.Id },
                { "Command.GuildID", context.Guild?.Id },
                { "Source", "Commands" }
            };
            if (!string.IsNullOrWhiteSpace(cmdName))
                state.Add("Command.Method", cmdName);
            if (handlerType != null)
                state.Add("Command.Handler", handlerType.Name);
            return this._log.BeginScope(state);
        }

        private Task OnLog(LogMessage logMessage)
        {
            this._log.Log(logMessage);
            return Task.CompletedTask;
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            this._cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            return Task.CompletedTask;
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            try { this._cts?.Cancel(); } catch { }
            this.Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            try { this._client.Ready -= OnClientReady; } catch { }
            try { this._client.SlashCommandExecuted -= OnSlashCommandAsync; } catch { }
            try { this._client.UserCommandExecuted -= OnUserCommandAsync; } catch { }
            try { this._client.MessageCommandExecuted -= OnMessageCommandAsync; } catch { }
            try { this._client.ButtonExecuted -= OnMenuCommandAsync; } catch { }
            try { this._client.SelectMenuExecuted -= OnButtonCommandAsync; } catch { }
            try { this._interactions.Log -= OnLog; } catch { }
            try { this._interactions?.Dispose(); } catch { }
            try { this._cts?.Dispose(); } catch { }
        }
    }
}