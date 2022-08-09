using Microsoft.AspNetCore.Components;

namespace TehGM.EinherjiBot.UI.Components.EntityInfo
{
    public abstract class DiscordEntityPickerBase<TEntity> : ComponentBase where TEntity : class
    {
        protected bool IsOpen { get; private set; } = false;

        private TaskCompletionSource<TEntity> _closeTcs;

        protected abstract Task OnOpeningAsync(CancellationToken cancellationToken);
        protected abstract Task OnClosingAsync(CancellationToken cancellationToken);

        public async Task<TEntity> OpenAsync(CancellationToken cancellationToken = default)
        {
            if (this._closeTcs != null)
                throw new InvalidOperationException($"{this.GetType().Name} can only be opened once at a time");
            await this.OnOpeningAsync(cancellationToken);
            this._closeTcs = new TaskCompletionSource<TEntity>();
            this.IsOpen = true;
            base.StateHasChanged();

            TEntity result = await this._closeTcs.Task;
            this._closeTcs = null;
            this.IsOpen = false;
            await this.OnClosingAsync(cancellationToken);
            base.StateHasChanged();
            return result;
        }

        public void PickEntity(TEntity value)
            => this._closeTcs.TrySetResult(value);

        public void Cancel()
            => this._closeTcs.TrySetResult(null);

        protected void OnDrawerOpenChanged(bool open)
        {
            if (!open)
                this.Cancel();
        }
    }
}
