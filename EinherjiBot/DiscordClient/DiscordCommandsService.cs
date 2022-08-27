using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Runtime.CompilerServices;
using TehGM.EinherjiBot.DiscordClient.Converters;

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

        public DiscordCommandsService(DiscordSocketClient client, IServiceProvider services,
            ILogger<DiscordCommandsService> log, IOptions<DiscordOptions> options)
        {
            this._client = client;
            this._services = services;
            this._log = log;
            this._options = options.Value;
            this._interactions = new InteractionService(this._client, new InteractionServiceConfig()
            {
                DefaultRunMode = RunMode.Sync,
                UseCompiledLambda = this._options.CompileCommands,
                AutoServiceScopes = false
            });

            AddTypeConverters(this._interactions);

            this._client.Ready += this.OnClientReady;
            this._client.SlashCommandExecuted += this.OnSlashCommandAsync;
            this._client.UserCommandExecuted += this.OnUserCommandAsync;
            this._client.MessageCommandExecuted += this.OnMessageCommandAsync;
            this._client.ButtonExecuted += this.OnButtonCommandAsync;
            this._client.SelectMenuExecuted += this.OnMenuCommandAsync;
            this._client.AutocompleteExecuted += this.OnAutocompleteAsync;
            this._interactions.SlashCommandExecuted += this.OnSlashCommandExecutedAsync;
            this._interactions.Log += this.OnLog;
        }

        private static void AddTypeConverters(InteractionService interactions)
        {
            interactions.AddTypeConverter<Guid>(new GuidTypeConverter());
        }

        private Task OnClientReady()
        {
            _ = Task.Run(async () =>
            {
                this._log.LogTrace("Loading all command modules");

                using IServiceScope scope = this._services.CreateScope();
                IDiscordAuthProvider authProvider = scope.ServiceProvider.GetRequiredService<IDiscordAuthProvider>();
                authProvider.User = DiscordSocketAuthContext.None;

                if (this._options.OverrideCommandsGuildID != null)
                {
                    this._log.LogDebug("Registering all commands for guild {GuildID}", this._options.OverrideCommandsGuildID.Value);
                    await this._interactions.AddModulesAsync(this.GetType().Assembly, scope.ServiceProvider);
                    await this._interactions.RegisterCommandsToGuildAsync(this._options.OverrideCommandsGuildID.Value).ConfigureAwait(false);
                }
                else
                {
                    IEnumerable<ModuleInfo> modules = await this._interactions.AddModulesAsync(this.GetType().Assembly, scope.ServiceProvider).ConfigureAwait(false);

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
            }, this._cts.Token);

            return Task.CompletedTask;
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
        private Task OnAutocompleteAsync(SocketAutocompleteInteraction interaction)
            => this.OnInteractionAsync(interaction);

        private async Task OnInteractionAsync(SocketInteraction interaction)
        {
            using IServiceScope scope = this._services.CreateScope();
            IDiscordAuthProvider authProvider = scope.ServiceProvider.GetRequiredService<IDiscordAuthProvider>();
            IDiscordAuthContext authContext = await authProvider.FromInteractionAsync(interaction, this._cts.Token);
            authProvider.User = authContext;

            EinherjiInteractionContext ctx = new EinherjiInteractionContext(this._client, interaction, authContext, this._cts.Token);
            using IDisposable logScope = this.BeginCommandScope(ctx, null, null);

            try
            {
                await this._interactions.ExecuteCommandAsync(ctx, scope.ServiceProvider);
            }
            catch (Exception ex)
            {
                try
                {
                    await HandleExceptionAsync(ex).ConfigureAwait(false);
                }
                catch { }
                throw;
            }

            Task HandleExceptionAsync(Exception exception)
            {
                string text = null;
                if ((exception as HttpException).IsMissingPermissions())
                    text = $"{EinherjiEmote.FailureSymbol} **Error**: Bot missing permissions. Please contact guild admin.";
                else if (exception is API.ApiException)
                {
                    if (!string.IsNullOrWhiteSpace(exception.Message))
                        text = $"{EinherjiEmote.FailureSymbol} **Error**: {exception.Message}";
                    else if (exception is API.BadRequestException)
                        text = $"{EinherjiEmote.FailureSymbol} **Damn.** Seems your command was malformed... somehow. {EinherjiEmote.BuzzHmm}";
                    else if (exception is API.AccessForbiddenException)
                        text = $"{EinherjiEmote.FailureSymbol} **Oof!** You have no permission to do that! {EinherjiEmote.GuyFawkes}";
                    else
                        text = $"{EinherjiEmote.FailureSymbol} **Oopsies!** Something was wrong with your command. {EinherjiEmote.FacepalmOutline}";
                }
                // for unknown exceptions, we want the Discord's default error behaviour
                else
                    return Task.CompletedTask;

                if (!interaction.HasResponded)
                    return interaction.RespondAsync(
                        text: text,
                        ephemeral: true,
                        options: ctx.CancellationToken.ToRequestOptions());
                else
                    return interaction.ModifyOriginalResponseAsync(msg =>
                    {
                        msg.Components = null;
                        msg.Embed = null;
                        msg.AllowedMentions = null;
                        msg.Embeds = null;
                        msg.Flags = new Optional<MessageFlags?>(MessageFlags.None);
                        msg.Content = text;
                    },
                    ctx.CancellationToken.ToRequestOptions());
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

        private Task OnSlashCommandExecutedAsync(SlashCommandInfo command, IInteractionContext context, IResult result)
        {
            if (result.IsSuccess)
                return Task.CompletedTask;
            if (result.Error != InteractionCommandError.UnmetPrecondition)
                return Task.CompletedTask;
            return context.Interaction.RespondAsync($"{EinherjiEmote.FailureSymbol} {result.ErrorReason}", options: this._cts.Token.ToRequestOptions());
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
            try { this._client.Ready -= this.OnClientReady; } catch { }
            try { this._client.SlashCommandExecuted -= this.OnSlashCommandAsync; } catch { }
            try { this._client.UserCommandExecuted -= this.OnUserCommandAsync; } catch { }
            try { this._client.MessageCommandExecuted -= this.OnMessageCommandAsync; } catch { }
            try { this._client.ButtonExecuted -= this.OnMenuCommandAsync; } catch { }
            try { this._client.SelectMenuExecuted -= this.OnButtonCommandAsync; } catch { }
            try { this._interactions.SlashCommandExecuted -= this.OnSlashCommandExecutedAsync; } catch { }
            try { this._interactions.Log -= this.OnLog; } catch { }
            try { this._interactions?.Dispose(); } catch { }
            try { this._cts?.Dispose(); } catch { }
        }
    }
}
