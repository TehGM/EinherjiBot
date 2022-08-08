using Microsoft.AspNetCore.Components;

namespace TehGM.EinherjiBot.UI.Components.EntityInfo
{
    public class EntityIdListBase<TEntity> : ComponentBase where TEntity : class
    {
        [Parameter]
        public bool AllowNewValues { get; set; } = true;
        [Parameter]
        public IList<ulong> Values { get; set; }
        [Parameter]
        public EventCallback<IList<ulong>> ValuesChanged { get; set; }

        public bool AllValuesValid => this.InvalidValues?.Any() != true && this.NewItemField?.IsError != true;

        protected EntityIdListItemBase<TEntity> NewItemField { get; set; }
        protected HashSet<ulong> InvalidValues { get; } = new HashSet<ulong>();

        protected virtual async Task OnValueChangedAsync(EntityIdListItemBase<TEntity>.ValueChangedEventArgs<ulong?> e)
        {
            int index = -1;
            bool anythingChanged = false;

            if (e.PreviousValue != null)
            {
                index = e.PreviousValue != null ? this.Values.IndexOf(e.PreviousValue.Value) : -1;
                if (index >= 0)
                    this.Values.RemoveAt(index);
                this.InvalidValues.Remove(e.PreviousValue.Value);
                anythingChanged = true;
            }
            if (e.NewValue != null && !this.Values.Contains(e.NewValue.Value))
            {
                if (index >= 0)
                    this.Values.Insert(index, e.NewValue.Value);
                else
                    this.Values.Add(e.NewValue.Value);

                if (!e.IsValid)
                    this.InvalidValues.Add(e.NewValue.Value);

                anythingChanged = true;
            }

            if (anythingChanged)
                await this.ValuesChanged.InvokeAsync(this.Values);
        }

        protected virtual async Task OnNewValueInputAsync(EntityIdListItemBase<TEntity>.ValueChangedEventArgs<ulong?> e)
        {
            if (e.IsValid)
            {
                if (e.NewValue != null && !this.Values.Contains(e.NewValue.Value))
                    this.Values.Add(e.NewValue.Value);
                this.NewItemField?.Clear();
            }
            await this.ValuesChanged.InvokeAsync(this.Values);
        }
    }
}
