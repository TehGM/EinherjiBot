using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.Security.Policies;

namespace TehGM.EinherjiBot.UI.Security.Policies
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class AuthorizeAdminAttribute : AuthorizeAttribute, IBotAuthorizationPolicyAttribute
    {
        public AuthorizeAdminAttribute()
            : base(typeof(AuthorizeAdmin)) { }
    }
}
