﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TehGM.EinherjiBot.CommandsProcessing.Checks;
using ParameterInfo = System.Reflection.ParameterInfo;

namespace TehGM.EinherjiBot.CommandsProcessing.Services
{
    public class RegexCommandInstance
    {
        public CommandDescriptor Descriptor { get; private set; }

        public Regex Regex { get; }
        public IEnumerable<Attribute> Attributes => this._attributes?.AsEnumerable();
        public Type ModuleType => _method.DeclaringType;
        public string MethodName => _method.Name;

        private readonly MethodInfo _method;
        private readonly ParameterInfo[] _params;
        private readonly IRegexCommandModuleProvider _moduleProvider;
        private ICollection<Attribute> _attributes;

        private RegexCommandInstance(Regex regex, MethodInfo method, IRegexCommandModuleProvider moduleProvider)
        {
            this.Regex = regex;

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

            result._attributes = new List<Attribute>();
            // first load base type attributes
            result.LoadCustomAttributes(method.DeclaringType);
            // then load method attributes (and let them overwrite class ones if necessary)
            result.LoadCustomAttributes(method);

            // pre-init if requested - this may be the case if module listens to some gateway events directly
            PersistentModuleAttribute persistent = method.DeclaringType.GetCustomAttribute<PersistentModuleAttribute>();
            if (persistent != null && persistent.PreInitialize)
                result._moduleProvider.GetModuleInstance(result);

            // build descriptor
            result.Descriptor = new CommandDescriptor(result);

            return result;
        }

        private void LoadCustomAttributes(ICustomAttributeProvider provider)
        {
            IEnumerable<object> attributes = provider.GetCustomAttributes(true);

            foreach (object attr in attributes)
            {
                if (attr is Attribute a)
                    this._attributes.Add(a);
            }
        }

        public async Task<CommandCheckResult> RunChecksAsync(CommandContext context, IServiceProvider services, CancellationToken cancellationToken = default)
        {
            foreach (CommandCheckAttribute commandCheck in this.Descriptor.CommandChecks)
            {
                CommandCheckResult result = await commandCheck.RunCheckAsync(context, services, cancellationToken).ConfigureAwait(false);
                if (result.ResultType == CommandCheckResultType.Skip || result.ResultType == CommandCheckResultType.Abort)
                    return result;
            }
            return CommandCheckResult.Success;
        }

        public async Task ExecuteAsync(CommandContext context, Match regexMatch, IServiceProvider services, CancellationToken cancellationToken = default)
        {
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
                else if (context.Guild != null && param.ParameterType.IsAssignableFrom(context.Guild.GetType()))
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
                            throw new InvalidOperationException($"Unsupported param type: {param.ParameterType.FullName}");
                    }
                }
                paramsValues[param.Position] = value;
            }

            // create class instance, or use pre-initialized if command has that flag
            cancellationToken.ThrowIfCancellationRequested();
            object instance = _moduleProvider.GetModuleInstance(this);

            // execute
            if (_method.Invoke(instance, paramsValues) is Task returnTask)
                await returnTask.ConfigureAwait(false);
        }
    }
}
