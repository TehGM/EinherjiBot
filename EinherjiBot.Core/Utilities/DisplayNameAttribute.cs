namespace TehGM.EinherjiBot
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class DisplayNameAttribute : Attribute
    {
        public string Name { get; }

        public DisplayNameAttribute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException(nameof(name));

            this.Name = name;
        }
    }
}
