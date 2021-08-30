using System;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    /// <summary>Represents module and additional info returned to the command handler.</summary>
    public class RegexCommandModule
    {
        /// <summary>The actual module isntance.</summary>
        public object Instance { get; }
        /// <summary>Whether instance is persistent.</summary>
        public bool IsPersistent { get; }
        /// <summary>Whether instance needs to be disposed after command execution.</summary>
        /// <remarks>This is true for instances that are IDisposable and are not persistent.</remarks>
        public bool NeedsDisposing => this.Instance is IDisposable && !this.IsPersistent;
        /// <summary>The module type.</summary>
        public Type Type => this.Instance.GetType();

        public RegexCommandModule(object instance, bool persistent)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            this.Instance = instance;
            this.IsPersistent = persistent;
        }

        public void DisposeInstance(bool force = false)
        {
            if (!this.NeedsDisposing && !force)
                return;

            if (this.Instance is IDisposable disposableInstance)
                disposableInstance.Dispose();
        }
    }
}
