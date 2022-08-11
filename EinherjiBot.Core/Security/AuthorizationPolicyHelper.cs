using TehGM.EinherjiBot.Security.Policies;

namespace TehGM.EinherjiBot.Security
{
    public static class AuthorizationPolicyHelper
    {
        public static IEnumerable<Type> AppendAuthorizePolicy(IEnumerable<Type> existingPolicies)
        {
            if (!existingPolicies.Contains(typeof(Authorize)))
            {
                List<Type> types = new List<Type>(existingPolicies.Count() + 1);
                types.Add(typeof(Authorize));
                types.AddRange(existingPolicies);
                return types;
            }
            else
                return existingPolicies;
        }
    }
}
