using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ParameterInfo = System.Reflection.ParameterInfo;

namespace TehGM.EinherjiBot.CommandsProcessing.Services
{
    public class RegexCommandInstance
    {
        public Regex Regex { get; }
        public int Priority { get; private set; }
        public IEnumerable<PreconditionAttribute> Preconditions { get; private set; }
        public Type ModuleType => _method.DeclaringType;
        public string MethodName => _method.Name;
        public RunMode RunMode
        {
            get => _runMode == RunMode.Default ? RunMode.Sync : _runMode;
            set => _runMode = value;
        }

        private RunMode _runMode = RunMode.Default;
        private readonly MethodInfo _method;
        private readonly ParameterInfo[] _params;
        private readonly IRegexCommandModuleProvider _moduleProvider;

        private RegexCommandInstance(Regex regex, MethodInfo method, IRegexCommandModuleProvider moduleProvider)
        {
            this.Regex = regex;
            this.Priority = 0;
            this.Preconditions = new List<PreconditionAttribute>();

            this._method = method;
            this._params = method.GetParameters();

            this._moduleProvider = moduleProvider;
        }

        public static RegexCommandInstance Build(MethodInfo method, RegexCommandAttribute regexAttribute, IServiceProvider services)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (regexAttribute == null)
                throw new ArgumentNullException(nameof(regexAttribute));

            // init instance
            CommandsOptions options = services.GetService<IOptions<CommandsOptions>>()?.Value;
            RegexOptions regexOptions = regexAttribute.RegexOptions;
            if (options?.CaseSensitive != true)
                regexOptions |= RegexOptions.IgnoreCase;
            IRegexCommandModuleProvider moduleProvider = services.GetRequiredService<IRegexCommandModuleProvider>();
            RegexCommandInstance result = new RegexCommandInstance(new Regex(regexAttribute.Pattern, regexOptions), method, moduleProvider);
            result.RunMode = options?.DefaultRunMode ?? RunMode.Default;

            // first load base type attributes
            result.LoadCustomAttributes(method.DeclaringType);
            // then load method attributes (and let them overwrite class ones if necessary)
            result.LoadCustomAttributes(method);

            // pre-init if requested - this may be the case if module listens to some gateway events directly
            PersistentModuleAttribute persistent = method.DeclaringType.GetCustomAttribute<PersistentModuleAttribute>();
            if (persistent != null && persistent.PreInitialize)
                result._moduleProvider.GetModuleInstance(result);

            return result;
        }

        private void LoadCustomAttributes(ICustomAttributeProvider provider)
        {
            IEnumerable<object> attributes = provider.GetCustomAttributes(true);

            foreach (object attr in attributes)
            {
                switch (attr)
                {
                    case PreconditionAttribute precondition:
                        (this.Preconditions as ICollection<PreconditionAttribute>).Add(precondition);
                        break;
                    case PriorityAttribute priority:
                        this.Priority = priority.Priority;
                        break;
                }
            }
        }

        public async Task<PreconditionResult> CheckPreconditionsAsync(ICommandContext context, IServiceProvider services)
        {
            foreach (PreconditionAttribute precondition in Preconditions)
            {
                // since we're piggy-backing on DNet's command's system that is really bad for extensibility, we have to improvise and pass null for command info
                // luckily none of the built-in preconditions seem to use it at this time anyway
                PreconditionResult result = await precondition.CheckPermissionsAsync(context, null, services).ConfigureAwait(false);
                if (!result.IsSuccess)
                    return result;
            }
            return PreconditionResult.FromSuccess();
        }

        public async Task<IResult> ExecuteAsync(ICommandContext context, int argPos, IServiceProvider services, CancellationToken cancellationToken = default)
        {
            // check regex
            string msg = context.Message.Content.Substring(argPos);
            Match regexMatch = this.Regex.Match(msg);
            if (regexMatch == null || !regexMatch.Success)
                return ExecuteResult.FromError(CommandError.ParseFailed, "Regex did not match");

            // build params
            cancellationToken.ThrowIfCancellationRequested();
            object[] paramsValues = new object[_params.Length];
            foreach (ParameterInfo param in _params)
            {
                object value = null;
                if (param.ParameterType.IsAssignableFrom(context.GetType()))
                    value = context;
                else if (param.ParameterType.IsAssignableFrom(typeof(Match)))
                    value = regexMatch;
                else if (param.ParameterType.IsAssignableFrom(context.Message.GetType()))
                    value = context.Message;
                else if (param.ParameterType.IsAssignableFrom(context.Guild.GetType()))
                    value = context.Guild;
                else if (param.ParameterType.IsAssignableFrom(context.Channel.GetType()))
                    value = context.Channel;
                else if (param.ParameterType.IsAssignableFrom(context.User.GetType()))
                    value = context.User;
                else if (param.ParameterType.IsAssignableFrom(context.Client.GetType()))
                    value = context.Client;
                else if (param.ParameterType.IsAssignableFrom(typeof(CancellationToken)))
                    value = cancellationToken;
                else
                {
                    value = services.GetService(param.ParameterType);
                    if (value == null)
                    {
                        if (param.IsOptional)
                            value = param.HasDefaultValue ? param.DefaultValue : null;
                        else
                            return ExecuteResult.FromError(CommandError.ObjectNotFound, $"Unsupported param type: {param.ParameterType.FullName}");
                    }
                }
                paramsValues[param.Position] = value;
            }

            // create class instance, or use pre-initialized if command has that flag
            cancellationToken.ThrowIfCancellationRequested();
            object instance = _moduleProvider.GetModuleInstance(this);

            // execute
            if (_method.Invoke(instance, paramsValues) is Task returnTask)
            {
                if (RunMode == RunMode.Sync)
                    await returnTask.ConfigureAwait(false);
                else
                    _ = Task.Run(async () => await returnTask.ConfigureAwait(false), cancellationToken);
            }
            return ExecuteResult.FromSuccess();
        }
    }
}
