using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace TehGM.EinherjiBot.API.Constraints
{
    public class UlongRouteConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (!values.TryGetValue(routeKey, out object routeValue))
                return false;

            return ulong.TryParse(routeValue.ToString(), out _);
        }
    }
}
