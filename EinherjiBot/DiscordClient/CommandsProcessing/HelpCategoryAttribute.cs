namespace TehGM.EinherjiBot.CommandsProcessing
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HelpCategoryAttribute : Attribute
    {
        public string CategoryName { get; }
        public int Priority { get; }

        public HelpCategoryAttribute(string categoryName, int priority)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                throw new ArgumentNullException(nameof(categoryName));

            this.CategoryName = categoryName;
            this.Priority = priority;
        }

        public HelpCategoryAttribute(string categoryName)
            : this(categoryName, 0) { }
    }
}
