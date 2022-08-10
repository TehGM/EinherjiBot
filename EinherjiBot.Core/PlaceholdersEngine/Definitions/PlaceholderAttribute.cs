namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
    public class PlaceholderAttribute : Attribute
    {
        public string Identifier { get; }
        public PlaceholderUsage AllowedContext { get; init; }

        public PlaceholderAttribute(string identifier, PlaceholderUsage allowedContext)
        {
            if (string.IsNullOrWhiteSpace(identifier))
                throw new ArgumentNullException(nameof(identifier));

            this.Identifier = identifier;
            this.AllowedContext = allowedContext;
        }
    }
}
