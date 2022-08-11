using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.UI.Security.Policies
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeAttribute : Attribute, IBotAuthorizationPolicyAttribute
    {
        public IEnumerable<Type> PolicyTypes { get; }

        public AuthorizeAttribute(params Type[] policies)
        {
            this.PolicyTypes = AuthorizationPolicyHelper.AppendAuthorizePolicy(policies);
        }
    }
}
