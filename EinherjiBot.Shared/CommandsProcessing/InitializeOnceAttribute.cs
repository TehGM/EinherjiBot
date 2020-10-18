using System;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    /// <summary>Tells regex command to not recreate class on every invokation, and use cached instance instead.</summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class InitializeOnceAttribute : Attribute
    {
    }
}
