using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TehGM.EinherjiBot.CommandsProcessing.Services
{
    public class RegexCommandHandler : IHostedService, IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly IOptionsMonitor<CommandsOptions> _commandOptions;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _log;
        private ICollection<RegexCommandInstance> _commands;
        private readonly SemaphoreSlim _lock;
        private CancellationToken _hostCancellationToken;

        public RegexCommandHandler(IServiceProvider serviceProvider, DiscordSocketClient client, IOptionsMonitor<CommandsOptions> commandOptions, ILogger<RegexCommandHandler> log)
        {
            this._client = client;
            this._commandOptions = commandOptions;
            this._serviceProvider = serviceProvider;
            this._log = log;
            this._commands = new List<RegexCommandInstance>();
            this._lock = new SemaphoreSlim(1, 1);

            _commandOptions.OnChange(async _ => await InitializeCommandsAsync());

            this._client.MessageReceived += HandleCommandAsync;
        }

        private async Task InitializeCommandsAsync()
        {
            await this._lock.WaitAsync(_hostCancellationToken).ConfigureAwait(false);
            try
            {
                this._log.LogDebug("Initializing commands");

                this._commands.Clear();
                CommandsOptions options = this._commandOptions.CurrentValue;
                CommandServiceConfig config = new CommandServiceConfig();
                foreach (Assembly asm in options.Assemblies)
                    this.AddAssembly(asm);
                foreach (Type t in options.Classes)
                    this.AddType(t.GetTypeInfo());

                this._commands = _commands.OrderByDescending(cmd => cmd.Priority).ToArray();
            }
            finally
            {
                this._lock.Release();
            }
        }

        private void AddAssembly(Assembly assembly)
        {
            IEnumerable<TypeInfo> types = assembly.DefinedTypes.Where(t => !t.IsAbstract && !t.ContainsGenericParameters
                && !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)) && Attribute.IsDefined(t, typeof(LoadRegexCommandsAttribute)));
            if (!types.Any())
            {
                _log.LogWarning("Cannot initialize Regex commands from assembly {AssemblyName} - no non-static non-abstract classes with {Attribute}", assembly.FullName, nameof(LoadRegexCommandsAttribute));
                return;
            }
            foreach (TypeInfo type in types)
                AddType(type);
        }

        private void AddType(TypeInfo type)
        {
            IEnumerable<MethodInfo> methods = type.DeclaredMethods.Where(m => !m.IsStatic && !Attribute.IsDefined(m, typeof(CompilerGeneratedAttribute)) && Attribute.IsDefined(m, typeof(RegexCommandAttribute)));
            if (!methods.Any())
            {
                _log.LogWarning("Cannot initialize Regex command from type {TypeName} - no method with {Attribute}", type.FullName, nameof(RegexCommandAttribute));
                return;
            }
            foreach (MethodInfo method in methods)
                AddMethod(method);
        }

        private void AddMethod(MethodInfo method)
        {
            IEnumerable<RegexCommandAttribute> attributes = method.GetCustomAttributes<RegexCommandAttribute>();
            if (!attributes.Any())
            {
                _log.LogWarning("Cannot initialize Regex command from {TypeName}'s method {MethodName} - {Attribute} missing", method.DeclaringType.FullName, method.Name, nameof(RegexCommandAttribute));
                return;
            }
            foreach (RegexCommandAttribute attribute in attributes)
                _commands.Add(RegexCommandInstance.Build(method, attribute, _serviceProvider));
        }

        private async Task HandleCommandAsync(SocketMessage msg)
        {
            // most of the implementation here taken from https://discord.foxbot.me/docs/guides/commands/intro.html
            // with my own pinch of customizations - TehGM

            // Don't process the command if it was a system message
            if (!(msg is SocketUserMessage message))
                return;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            CommandsOptions options = this._commandOptions.CurrentValue;
            // only execute if not a bot message
            if (!options.AcceptBotMessages && message.Author.IsBot)
                return;
            // get prefix and argPos
            int argPos = 0;
            bool requirePrefix = msg.Channel is SocketGuildChannel ? options.RequirePublicMessagePrefix : options.RequirePrivateMessagePrefix;
            bool hasStringPrefix = message.HasStringPrefix(options.Prefix, ref argPos);
            bool hasMentionPrefix = false;
            if (!hasStringPrefix)
                hasMentionPrefix = message.HasMentionPrefix(_client.CurrentUser, ref argPos);

            // if prefix not found but is required, return
            if (requirePrefix && (!string.IsNullOrWhiteSpace(options.Prefix) && !hasStringPrefix) && (options.AcceptMentionPrefix && !hasMentionPrefix))
                return;

            // Create a WebSocket-based command context based on the message
            SocketCommandContext context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.

            // Keep in mind that result does not indicate a return value
            // rather an object stating if the command executed successfully.
            await _lock.WaitAsync(_hostCancellationToken).ConfigureAwait(false);
            try
            {
                foreach (RegexCommandInstance command in _commands)
                {
                    try
                    {
                        ExecuteResult result = (ExecuteResult)await command.ExecuteAsync(
                            context: context,
                            argPos: argPos,
                            services: _serviceProvider,
                            cancellationToken: _hostCancellationToken)
                            .ConfigureAwait(false);
                        if (result.IsSuccess)
                            return;
                    }
                    catch (Exception ex) when (ex.LogAsError(_log, "Unhandled Exception when executing command {MethodName}", command.MethodName)) { return; }
                }
            }
            finally
            {
                _lock.Release();
            }
        }

        Task IHostedService.StartAsync(CancellationToken cancellationToken)
        {
            this._hostCancellationToken = cancellationToken;
            return InitializeCommandsAsync();
        }

        Task IHostedService.StopAsync(CancellationToken cancellationToken)
        {
            this.Dispose();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            this._client.MessageReceived -= HandleCommandAsync;
            this._lock.Dispose();
        }
    }
}
