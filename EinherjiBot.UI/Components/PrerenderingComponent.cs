using Microsoft.AspNetCore.Components;

namespace TehGM.EinherjiBot.UI
{
    public abstract class PrerenderingComponent : ComponentBase, IDisposable
    {
        [Inject]
        private IRenderLocation RenderLocation { get; init; }
        [Inject]
        protected PersistentComponentState PrerenderingState { get; init; }
        private PersistingComponentStateSubscription _prerenderingSubscription;

        protected override Task OnInitializedAsync()
        {
            if (this.RenderLocation.IsServer)
                this._prerenderingSubscription = this.PrerenderingState.RegisterOnPersisting(this.PersistAsync);
            return Task.CompletedTask;
        }

        protected abstract Task PersistAsync();

        protected void PersistItem<T>(Type componentType, string itemName, T value)
            => this.PrerenderingState.PersistAsJson(this.BuildKey(componentType, itemName), value);
        protected void PersistItem<T>(string itemName, T value)
            => this.PersistItem(this.GetType(), itemName, value);

        protected bool TryGetItem<T>(Type componentType, string itemName, out T value)
            => this.PrerenderingState.TryTakeFromJson(this.BuildKey(componentType, itemName), out value);
        protected bool TryGetItem<T>(string itemName, out T value)
            => this.TryGetItem(this.GetType(), itemName, out value);

        private string BuildKey(Type type, string name)
            => $"{type.FullName}+{name}";

        public virtual void Dispose()
        {
            try { this._prerenderingSubscription.Dispose(); } catch { }
        }
    }
}
