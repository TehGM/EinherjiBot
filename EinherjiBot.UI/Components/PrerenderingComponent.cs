using Microsoft.AspNetCore.Components;

namespace TehGM.EinherjiBot.UI
{
    public abstract class PrerenderingComponent : ComponentBase, IDisposable
    {
        [Inject]
        protected PersistentComponentState PrerenderingState { get; init; }
        private PersistingComponentStateSubscription _prerenderingSubscription;

        protected override Task OnInitializedAsync()
        {
            this._prerenderingSubscription = this.PrerenderingState.RegisterOnPersisting(this.PersistAsync);
            return Task.CompletedTask;
        }

        protected abstract Task PersistAsync();

        public virtual void Dispose()
        {
            try { this._prerenderingSubscription.Dispose(); } catch { }
        }
    }
}
