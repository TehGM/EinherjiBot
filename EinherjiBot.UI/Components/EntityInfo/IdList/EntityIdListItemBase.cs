using Microsoft.AspNetCore.Components;

namespace TehGM.EinherjiBot.UI.Components.EntityInfo.IdList
{
    public abstract class EntityIdListItemBase<TEntity> : ComponentBase where TEntity : class
    {
        [Parameter]
        public bool ReadOnly { get; set; }
        [Parameter]
        public ulong? Value { get; set; }
        [Parameter]
        public EventCallback<ValueChangedEventArgs<ulong?>> ValueChanged { get; set; }

        protected abstract Task<TEntity> FindEntityAsync(ulong id);
        protected TEntity FoundEntity { get; private set; }

        public bool IsEmpty => this.Value == null;
        public bool IsValid => this.FoundEntity != null;
        public bool IsError => !this.IsEmpty && !this.IsValid;


        protected override async Task OnParametersSetAsync()
        {
            await this.ValidateAndLookupAsync();
            await base.OnParametersSetAsync();
        }

        public Task SetValueAsync(ulong? value)
            => this.OnValueChangedAsync(value);

        public void Clear()
        {
            this.Value = null;
            this.FoundEntity = null;
        }

        protected virtual async Task OnValueChangedAsync(ulong? value)
        {
            if (this.Value == value)
                return;

            ulong? previous = this.Value;
            this.Value = value;
            await this.ValidateAndLookupAsync();
            await this.ValueChanged.InvokeAsync(new ValueChangedEventArgs<ulong?>(previous, value, this.IsValid));
        }

        protected virtual async Task ValidateAndLookupAsync()
        {
            this.FoundEntity = null;

            if (this.IsEmpty)
                return;
            if (ValidateRange(this.Value.Value))
                this.FoundEntity = await this.FindEntityAsync(this.Value.Value);
        }

        protected static bool ValidateRange(ulong id)
        {
            if (id < 10000000000000000)
                return false;
            if (id > 5000000000000000000)
                return false; 
            return true;
        }

        public record ValueChangedEventArgs<T>(T PreviousValue, T NewValue, bool IsValid);
    }
}
