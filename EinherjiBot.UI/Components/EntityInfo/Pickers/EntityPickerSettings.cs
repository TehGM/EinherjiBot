namespace TehGM.EinherjiBot.UI.Components.EntityInfo.Pickers
{
    public class EntityPickerSettings
    {
        /// <summary>If set, only these guilds will be included in the picker.</summary>
        public IEnumerable<ulong> ShownGuildIDs { get; init; }
    }
}
