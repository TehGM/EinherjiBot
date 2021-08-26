using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing.Checks;

namespace TehGM.EinherjiBot.CommandsProcessing.Services
{
    public class RegexCommandHandler : CommandHandlerBase
    {
        public ICollection<RegexCommandInstance> Commands { get; private set; }

        public RegexCommandHandler(IServiceProvider serviceProvider, DiscordClient client, IOptionsMonitor<CommandsOptions> commandOptions, ILogger<RegexCommandHandler> log)
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

                this.Commands = Commands.OrderByDescending(cmd => cmd.Descriptor.Priority).ToArray();
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
                Commands.Add(new RegexCommandInstance(method, attribute, _serviceProvider));
        }

        protected override async Task HandleCommandAsync(MessageCreateEventArgs e, int argPos)
        {
            string msg = e.Message.Content;
            if (argPos > 0)
                msg = msg.Substring(argPos);

            await _lock.WaitAsync(_hostCancellationToken).ConfigureAwait(false);
            try
            {
                foreach (RegexCommandInstance command in Commands)
                {
                    // first check if command matches at all
                    Match regexMatch = command.Regex.Match(msg);
                    if (regexMatch == null || !regexMatch.Success)
                        continue;

                    // start context and log scope
                    CommandContext context = new CommandContext(command.Descriptor, e, _client);
                    using IDisposable logScope = _log.BeginCommandScope(context, command.ModuleType, command.MethodName);
                    try
                    {
                        // run command checks
                        CommandCheckResult result = await command.RunChecksAsync(context, this._serviceProvider, this._hostCancellationToken).ConfigureAwait(false);
                        if (result.ResultType != CommandCheckResultType.Continue)
                        {
                            base.LogCommandCheck(result, $"{command.ModuleType.Name}.{command.MethodName}");
                            if (result.ResultType == CommandCheckResultType.Skip)
                                continue;
                            if (result.ResultType == CommandCheckResultType.Abort)
                                break;
                        }

                        // execute the command
                        await command.ExecuteAsync(
                            context: context,
                            regexMatch: regexMatch,
                            services: this._serviceProvider,
                            cancellationToken: this._hostCancellationToken)
                            .ConfigureAwait(false);
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
