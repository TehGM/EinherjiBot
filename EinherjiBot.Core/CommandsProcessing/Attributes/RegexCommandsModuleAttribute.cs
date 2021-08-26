using System;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RegexCommandsModuleAttribute : Attribute
    {
        public bool SingletonScoped { get; set; }

        public RegexCommandsModuleAttribute() { }
    }
}
