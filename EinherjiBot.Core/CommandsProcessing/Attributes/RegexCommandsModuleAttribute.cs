using System;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class RegexCommandsModuleAttribute : Attribute
    {
        public bool IsPersistent { get; set; }
        public bool PreInitialize { get; set; }

        public RegexCommandsModuleAttribute() { }
    }
}
