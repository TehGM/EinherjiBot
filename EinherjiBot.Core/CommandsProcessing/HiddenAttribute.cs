using System;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HiddenAttribute : Attribute
    {
        public bool Hide { get; }

        public HiddenAttribute(bool hide)
        {
            this.Hide = hide;
        }

        public HiddenAttribute()
            : this(true) { }
    }
}
