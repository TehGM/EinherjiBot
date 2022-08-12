using Discord;

namespace TehGM.EinherjiBot.UI.Components.EntityInfo.Picker
{
    public class ChannelPickerSettings : EntityPickerSettings
    {
        /// <summary>If set, only these channel types will be included in the picker.</summary>
        public IEnumerable<ChannelType> ShownChannelTypes { get; init; }
        /// <summary>If set, only these channel types will be pickable. Others will still be displayed for hierarchy purposes.</summary>
        public IEnumerable<ChannelType> PickableChannelTypes { get; init; }
    }
}
