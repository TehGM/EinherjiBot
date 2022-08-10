namespace TehGM.EinherjiBot
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class DescriptionAttribute : Attribute
    {
        public string Description { get; }

        public DescriptionAttribute(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                throw new ArgumentNullException(nameof(description));

            this.Description = description;
        }
    }
}
