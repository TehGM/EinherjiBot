using System;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    public class RegexCommandModule
    {
        public object Instance { get; }
        public bool IsPersistent { get; }
        public bool NeedsDisposing { get; }

        public Type Type => this.Instance.GetType();

        public RegexCommandModule(object instance, bool persistent, bool dispose)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            this.Instance = instance;
            this.IsPersistent = persistent;
            this.NeedsDisposing = dispose;
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
