namespace TehGM.EinherjiBot.PlaceholdersEngine
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class PlaceholderPropertyAttribute : Attribute
    {
        public string Name { get; }
        public bool IsRequired { get; init; }
        public IDType IDType { get; init; }

        public PlaceholderPropertyAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            this.Name = name;
        }
    }
}
