using System;
using System.Collections.Generic;
using System.Linq;
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
        public RunMode RunMode
        {
            get => _runMode == RunMode.Default ? RunMode.Sync : _runMode;
            set => _runMode = value;
        }

        private RunMode _runMode = RunMode.Default;
        private readonly MethodInfo _method;
        private readonly ParameterInfo[] _params;
        private readonly InstanceClassCreator _cachedCreator;
        private object _cachedInstance;

        private RegexCommandInstance(Regex regex, MethodInfo method, IServiceProvider services)
        {
            this.Regex = regex;
            this.Priority = 0;
            this.Preconditions = new List<PreconditionAttribute>();

            this._method = method;
            this._params = method.GetParameters();

            // init instance creator
            IEnumerable<ConstructorInfo> constructors = _method.DeclaringType
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .OrderByDescending(ctor => ctor.GetParameters().Length);
            foreach (ConstructorInfo ctor in constructors)
                _cachedCreator = TryGetCreator(ctor, services);
            if (_cachedCreator == null)
                throw new InvalidOperationException($"Cannot create {_method.DeclaringType.FullName} - none of the constructors can have its dependencies resolved");
        }

        public static RegexCommandInstance Build(MethodInfo method, RegexCommandAttribute regexAttribute, IServiceProvider services)
        {
            if (method == null)
                throw new ArgumentNullException(nameof(method));
            if (regexAttribute == null)
                throw new ArgumentNullException(nameof(regexAttribute));

            // init instance
            CommandOptions options = services.GetService<IOptionsSnapshot<CommandOptions>>()?.Value;
            RegexOptions regexOptions = regexAttribute.RegexOptions;
            if (options?.CaseSensitive != true)
                regexOptions |= RegexOptions.IgnoreCase;
            RegexCommandInstance result = new RegexCommandInstance(new Regex(regexAttribute.Pattern, regexOptions), method, services);
            result.RunMode = options?.DefaultRunMode ?? RunMode.Default;

            // first load base type attributes
            result.LoadCustomAttributes(method.DeclaringType);
            // then load method attributes (and let them overwrite class ones if necessary)
            result.LoadCustomAttributes(method);

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
                    case InitializeOnceAttribute _:
                        this._cachedInstance = _cachedCreator.Create();
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
            object instance = _cachedInstance ?? _cachedCreator.Create();

            // execute
            try
            {
                if (_method.Invoke(instance, paramsValues) is Task returnTask)
                {
                    if (RunMode == RunMode.Sync)
                        await returnTask.ConfigureAwait(false);
                    else
                        _ = Task.Run(async () => await returnTask.ConfigureAwait(false), cancellationToken);
                }
                return ExecuteResult.FromSuccess();
            }
            catch (Exception ex)
            {
                return ExecuteResult.FromError(ex);
            }
        }

        private InstanceClassCreator TryGetCreator(ConstructorInfo constructor, IServiceProvider services)
        {
            ParameterInfo[] ctorParams = constructor.GetParameters();
            object[] paramsValues = new object[ctorParams.Length];
            foreach (ParameterInfo param in ctorParams)
            {
                object value = services.GetService(param.ParameterType);
                if (value == null)
                {
                    if (param.IsOptional)
                        value = param.HasDefaultValue ? param.DefaultValue : null;
                    else
                        return null;
                }
                paramsValues[param.Position] = value;
            }
            return new InstanceClassCreator(constructor, paramsValues);
        }

        /// <summary>Info on matched constructor in params. For caching purposes.</summary>
        private class InstanceClassCreator
        {
            private readonly ConstructorInfo _ctor;
            private readonly object[] _params;

            public InstanceClassCreator(ConstructorInfo ctor, object[] parameters)
            {
                this._ctor = ctor;
                this._params = parameters;
            }

            public object Create()
                => _ctor.Invoke(_params);
        }
    }
}
