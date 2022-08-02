using TehGM.EinherjiBot.Security.Policies;

namespace TehGM.EinherjiBot.UI.Security.Policies
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeAttribute : Attribute
    {
        public IEnumerable<Type> PolicyTypes { get; }

        public AuthorizeAttribute(params Type[] policies)
        {
            if (!policies.Contains(typeof(Authorize)))
            {
                List<Type> types = new List<Type>(policies.Length + 1);
                types.Add(typeof(Authorize));
                types.AddRange(policies);
                this.PolicyTypes = types;
            }
            else
                this.PolicyTypes = policies;
        }
    }
}
