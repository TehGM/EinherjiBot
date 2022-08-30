namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    public interface IPlaceholderContextProvider
    {
        public PlaceholderConvertContext Context { get; set; }

        public PlaceholderUsage ContextType => this.Context.ContextType;
        public ulong? CurrentUserID => this.Context.CurrentUserID;
        public ulong? CurrentChannelID => this.Context.CurrentChannelID;
        public ulong? CurrentGuildID => this.Context.CurrentGuildID;

        void Clear() => this.Context = null;
    }
}
