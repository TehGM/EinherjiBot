using System.Reflection;
using System.Runtime.CompilerServices;
using Discord.Commands;
using Discord.WebSocket;

namespace TehGM.EinherjiBot.CommandsProcessing.Services
{
    public class RegexCommandHandler : CommandHandlerBase
    {
        public ICollection<RegexCommandInstance> Commands { get; private set; }

        public RegexCommandHandler(IServiceProvider serviceProvider, DiscordSocketClient client, IOptionsMonitor<CommandsOptions> commandOptions, ILogger<RegexCommandHandler> log)
            : base(serviceProvider, client, commandOptions, log)
        {
            this.Commands = new List<RegexCommandInstance>();
        }

        protected override async Task InitializeCommandsAsync()
        {
            await this._lock.WaitAsync(_hostCancellationToken).ConfigureAwait(false);
            try
            {
                this._log.LogDebug("Initializing commands");

                this.Commands.Clear();
                CommandsOptions options = this._commandOptions.CurrentValue;
                foreach (Assembly asm in options.Assemblies)
                    this.AddAssembly(asm);
                foreach (Type t in options.Classes)
                    this.AddType(t.GetTypeInfo());

                this.Commands = Commands.OrderByDescending(cmd => cmd.Priority).ToArray();
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
                Commands.Add(RegexCommandInstance.Build(method, attribute, _serviceProvider));
        }

        protected override async Task HandleCommandAsync(SocketCommandContext context, int argPos)
        {
            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.

            // Keep in mind that result does not indicate a return value
            // rather an object stating if the command executed successfully.
            await _lock.WaitAsync(_hostCancellationToken).ConfigureAwait(false);
            try
            {
                foreach (RegexCommandInstance command in Commands)
                {
                    using IDisposable logScope = _log.BeginCommandScope(context, command.ModuleType, command.MethodName);
                    try
                    {
                        IResult preconditionsResult = await command.CheckPreconditionsAsync(context, _serviceProvider);
                        if (!preconditionsResult.IsSuccess)
                            continue;
                        ExecuteResult result = (ExecuteResult)await command.ExecuteAsync(
                            context: context,
                            argPos: argPos,
                            services: _serviceProvider,
                            cancellationToken: _hostCancellationToken)
                            .ConfigureAwait(false);
                        if (result.IsSuccess)
                            return;
                    }
                    catch (OperationCanceledException) { return; }
                    catch (Exception ex) when (ex.LogAsError(_log, "Unhandled Exception when executing command {MethodName}", command.MethodName)) { return; }
                }
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}
