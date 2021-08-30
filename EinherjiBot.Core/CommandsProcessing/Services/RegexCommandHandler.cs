using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing.Checks;

namespace TehGM.EinherjiBot.CommandsProcessing.Services
{
    public class RegexCommandHandler : CommandHandlerBase
    {
        public ICollection<RegexCommandInstance> Commands { get; private set; }

        private readonly IRegexCommandModuleProvider _moduleProvider;

        public RegexCommandHandler(IServiceProvider serviceProvider, IRegexCommandModuleProvider moduleProvider, DiscordClient client, IOptionsMonitor<CommandsOptions> commandOptions, ILogger<RegexCommandHandler> log)
            : base(serviceProvider, client, commandOptions, log)
        {
            this.Commands = new List<RegexCommandInstance>();
            this._moduleProvider = moduleProvider;
        }

        protected override async Task InitializeCommandsAsync()
        {
            await this.Lock.WaitAsync(HostCancellationToken).ConfigureAwait(false);
            try
            {
                this.Log.LogDebug("Initializing commands");

                this.Commands.Clear();
                CommandsOptions options = this.CommandOptions.CurrentValue;
                foreach (Assembly asm in options.Assemblies)
                    this.AddAssembly(asm);
                foreach (Type t in options.Classes)
                    this.AddType(t.GetTypeInfo());

                this.Commands = Commands.OrderByDescending(cmd => cmd.Descriptor.Priority).ToArray();
            }
            finally
            {
                this.Lock.Release();
            }
        }

        private void AddAssembly(Assembly assembly)
        {
            IEnumerable<TypeInfo> types = assembly.DefinedTypes.Where(t => !t.IsAbstract && !t.ContainsGenericParameters
                && !Attribute.IsDefined(t, typeof(CompilerGeneratedAttribute)) && Attribute.IsDefined(t, typeof(RegexCommandsModuleAttribute)));
            if (!types.Any())
            {
                this.Log.LogWarning("Cannot initialize Regex commands from assembly {AssemblyName} - no non-static non-abstract classes with {Attribute}", assembly.FullName, nameof(RegexCommandsModuleAttribute));
                return;
            }
            foreach (TypeInfo type in types)
                this.AddType(type);
        }

        private void AddType(TypeInfo type)
        {
            IEnumerable<MethodInfo> methods = type.DeclaredMethods.Where(m => !m.IsStatic && !Attribute.IsDefined(m, typeof(CompilerGeneratedAttribute)) && Attribute.IsDefined(m, typeof(RegexCommandAttribute)));
            if (!methods.Any())
            {
                this.Log.LogWarning("Cannot initialize Regex command from type {TypeName} - no method with {Attribute}", type.FullName, nameof(RegexCommandAttribute));
                return;
            }
            foreach (MethodInfo method in methods)
                this.AddMethod(method);
        }

        private void AddMethod(MethodInfo method)
        {
            IEnumerable<RegexCommandAttribute> attributes = method.GetCustomAttributes<RegexCommandAttribute>();
            if (!attributes.Any())
            {
                this.Log.LogWarning("Cannot initialize Regex command from {TypeName}'s method {MethodName} - {Attribute} missing", method.DeclaringType.FullName, method.Name, nameof(RegexCommandAttribute));
                return;
            }
            foreach (RegexCommandAttribute attribute in attributes)
            {
                CommandsOptions options = this.ServiceProvider.GetRequiredService<IOptions<CommandsOptions>>().Value;
                RegexCommandInstance instance = new RegexCommandInstance(method, attribute, options);
                this.Commands.Add(instance);

                // preinitialize singleton
                RegexCommandsModuleAttribute moduleAttribute = method.DeclaringType.GetCustomAttribute<RegexCommandsModuleAttribute>();
                if (moduleAttribute != null && moduleAttribute.SingletonScoped)
                    this._moduleProvider.GetModuleInstance(instance);
            }
        }

        protected override async Task HandleCommandAsync(MessageCreateEventArgs e, int argPos)
        {
            string msg = e.Message.Content;
            if (argPos > 0)
                msg = msg.Substring(argPos);

            await this.Lock.WaitAsync(HostCancellationToken).ConfigureAwait(false);
            try
            {
                foreach (RegexCommandInstance command in this.Commands)
                {
                    // first check if command matches at all
                    Match regexMatch = command.Regex.Match(msg);
                    if (regexMatch == null || !regexMatch.Success)
                        continue;

                    // start context and log scope
                    CommandContext context = new CommandContext(command.Descriptor, e, this.Client);
                    using IDisposable logScope = Log.BeginCommandScope(context, command.ModuleType, command.MethodName);
                    try
                    {
                        // run command checks
                        CommandCheckResult result = await command.RunChecksAsync(context, this.ServiceProvider, this.HostCancellationToken).ConfigureAwait(false);
                        if (result.ResultType != CommandCheckResultType.Continue)
                        {
                            base.LogCommandCheck(result, $"{command.ModuleType.Name}.{command.MethodName}");
                            if (result.ResultType == CommandCheckResultType.Skip)
                                continue;
                            if (result.ResultType == CommandCheckResultType.Abort)
                                break;
                        }

                        // get module for the command
                        RegexCommandModule module = this._moduleProvider.GetModuleInstance(command);

                        // execute the command
                        await command.ExecuteAsync(
                            context: context,
                            regexMatch: regexMatch,
                            module: module.Instance,
                            services: this.ServiceProvider,
                            cancellationToken: this.HostCancellationToken)
                            .ConfigureAwait(false);

                        module.DisposeInstance();
                        return;
                    }
                    catch (OperationCanceledException) { return; }
                    catch (Exception ex) when (ex.LogAsError(this.Log, "Unhandled Exception when executing command {MethodName}", command.MethodName)) { return; }
                }
            }
            finally
            {
                this.Lock.Release();
            }
        }
    }
}
