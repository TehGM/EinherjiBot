using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace TehGM.EinherjiBot.CommandsProcessing.Services
{
    public class RegexComandModuleProvider : IRegexCommandModuleProvider, IDisposable
    {
        private readonly IServiceProvider _services;
        private readonly IDictionary<Type, RegexCommandModule> _persistentInstances;
        private readonly IDictionary<RegexCommandInstance, RegexCommandModuleInfo> _knownModules;

        public RegexComandModuleProvider(IServiceProvider services)
        {
            this._services = services;
            this._persistentInstances = new Dictionary<Type, RegexCommandModule>();
            this._knownModules = new Dictionary<RegexCommandInstance, RegexCommandModuleInfo>();
        }

        public RegexCommandModule GetModuleInstance(RegexCommandInstance commandInstance)
        {
            // check persistent ones to not recreate again
            if (this._persistentInstances.TryGetValue(commandInstance.ModuleType, out RegexCommandModule instance))
                return instance;

            // init module info
            RegexCommandModuleInfo moduleInfo;
            if (!this._knownModules.TryGetValue(commandInstance, out moduleInfo))
            {
                IEnumerable<ConstructorInfo> constructors = commandInstance.ModuleType
                    .GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .OrderByDescending(ctor => ctor.GetParameters().Length);
                foreach (ConstructorInfo ctor in constructors)
                {
                    moduleInfo = InitializeModuleInfo(ctor);
                    if (moduleInfo != null)
                    {
                        this._knownModules.Add(commandInstance, moduleInfo);
                        break;
                    }
                }
                if (moduleInfo == null)
                    throw new InvalidOperationException($"Cannot create {commandInstance.ModuleType.FullName} - none of the constructors can have its dependencies resolved");
            }

            // create a new instance, cache it if it's persistent
            instance = moduleInfo.CreateInstance();
            if (moduleInfo.IsPersistent)
                this._persistentInstances.Add(commandInstance.ModuleType, instance);

            return instance;
        }

        public void Dispose()
        {
            foreach (RegexCommandModule module in this._persistentInstances.Values)
                module.DisposeInstance(force: true);
            this._persistentInstances.Clear();
            this._knownModules.Clear();
        }

        private RegexCommandModuleInfo InitializeModuleInfo(ConstructorInfo constructor)
        {
            using IServiceScope scope = _services.CreateScope();

            ParameterInfo[] ctorParams = constructor.GetParameters();
            object[] paramsValues = new object[ctorParams.Length];
            foreach (ParameterInfo param in ctorParams)
            {
                object value = scope.ServiceProvider.GetService(param.ParameterType);
                if (value == null)
                {
                    if (param.IsOptional)
                        value = param.HasDefaultValue ? param.DefaultValue : null;
                    else
                        return null;
                }
                paramsValues[param.Position] = value;
            }
            return new RegexCommandModuleInfo(constructor, paramsValues);
        }

        private class RegexCommandModuleInfo
        {
            public Type Type { get; }
            public bool IsPersistent { get; }
            private readonly ConstructorInfo _ctor;
            private readonly object[] _params;

            public RegexCommandModuleInfo(ConstructorInfo ctor, object[] parameters)
            {
                this._ctor = ctor;
                this._params = parameters;

                this.Type = ctor.DeclaringType;
                RegexCommandsModuleAttribute moduleAttribute = this.Type.GetCustomAttribute<RegexCommandsModuleAttribute>();
                this.IsPersistent = moduleAttribute.SingletonScoped;
            }

            public RegexCommandModule CreateInstance()
            {
                object instance = _ctor.Invoke(_params);
                bool dispose = instance is IDisposable && !this.IsPersistent;
                return new RegexCommandModule(instance, this.IsPersistent, dispose);
            }
        }
    }
}
