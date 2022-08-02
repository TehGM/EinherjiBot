using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using System.Diagnostics.CodeAnalysis;
using TehGM.EinherjiBot.Caching;
using AuthorizeAttribute = TehGM.EinherjiBot.UI.Security.Policies.AuthorizeAttribute;

namespace TehGM.EinherjiBot.UI.Security.Components
{
    public class PolicyAuthorizeRouteView : RouteView
    {
        [Inject]
        private IEntityCache<Type, IEnumerable<Type>> PolicyCache { get; set; }

        [Parameter]
        public RenderFragment<AuthenticationState> NotAuthorized { get; set; }
        [Parameter]
        public RenderFragment Authorizing { get; set; }
        [Parameter]
        public object Resource { get; set; }

        private readonly RenderFragment _renderAuthorizeRouteViewCoreDelegate;
        private readonly RenderFragment<AuthenticationState> _renderAuthorizedDelegate;
        private readonly RenderFragment<AuthenticationState> _renderNotAuthorizedDelegate;
        private readonly RenderFragment _renderAuthorizingDelegate;

        // caching approach taken from https://github.com/dotnet/aspnetcore/blob/main/src/Components/Authorization/src/AuthorizeRouteView.cs
        public PolicyAuthorizeRouteView()
        {
            // Cache the rendering delegates so that we only construct new closure instances
            // when they are actually used (e.g., we never prepare a RenderFragment bound to
            // the NotAuthorized content except when you are displaying that particular state)
            RenderFragment renderBaseRouteViewDelegate = builder => base.Render(builder);
            this._renderAuthorizedDelegate = authenticateState => renderBaseRouteViewDelegate;
            this._renderNotAuthorizedDelegate = authenticationState => builder => this.RenderNotAuthorizedInDefaultLayout(builder, authenticationState);
            this._renderAuthorizingDelegate = this.RenderAuthorizingInDefaultLayout;
            this._renderAuthorizeRouteViewCoreDelegate = this.RenderAuthorizeRouteViewCore;
        }

        private void RenderAuthorizeRouteViewCore(RenderTreeBuilder builder)
        {
            builder.OpenComponent<PolicyAuthorizeView>(0);
            builder.AddAttribute(1, nameof(PolicyAuthorizeView.RouteData), base.RouteData);
            builder.AddAttribute(2, nameof(PolicyAuthorizeView.Authorized), this._renderAuthorizedDelegate);
            builder.AddAttribute(3, nameof(PolicyAuthorizeView.Authorizing), this._renderAuthorizingDelegate);
            builder.AddAttribute(4, nameof(PolicyAuthorizeView.NotAuthorized),this. _renderNotAuthorizedDelegate);
            builder.AddAttribute(5, nameof(PolicyAuthorizeView.Resource), this.Resource);
            builder.AddAttribute(6, nameof(PolicyAuthorizeView.PolicyTypes), this.GetRoutePolicies());
            builder.CloseComponent();
        }

        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2111:RequiresUnreferencedCode",
            Justification = "OpenComponent already has the right set of attributes")]
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2110:RequiresUnreferencedCode",
            Justification = "OpenComponent already has the right set of attributes")]
        [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2118:RequiresUnreferencedCode",
            Justification = "OpenComponent already has the right set of attributes")]
        private void RenderContentInDefaultLayout(RenderTreeBuilder builder, RenderFragment content)
        {
            builder.OpenComponent<LayoutView>(0);
            builder.AddAttribute(1, nameof(LayoutView.Layout), base.DefaultLayout);
            builder.AddAttribute(2, nameof(LayoutView.ChildContent), content);
            builder.CloseComponent();
        }

        private void RenderNotAuthorizedInDefaultLayout(RenderTreeBuilder builder, AuthenticationState authenticationState)
            => this.RenderContentInDefaultLayout(builder, this.NotAuthorized(authenticationState));

        private void RenderAuthorizingInDefaultLayout(RenderTreeBuilder builder)
            => this.RenderContentInDefaultLayout(builder, this.Authorizing);

        protected override void Render(RenderTreeBuilder builder)
            => this._renderAuthorizeRouteViewCoreDelegate(builder);

        private IEnumerable<Type> GetRoutePolicies()
        {
            Type pageType = base.RouteData.PageType;
            if (this.PolicyCache.TryGet(pageType, out IEnumerable<Type> policies))
                return policies;

            DisabledExpiration expiration = new DisabledExpiration();
            List<Type> foundPolicies = null;
            object[] attributes = pageType.GetCustomAttributes(inherit: true);
            foreach (object attr in attributes)
            {
                if (attr is IAllowAnonymous)
                {
                    this.PolicyCache.AddOrReplace(pageType, Enumerable.Empty<Type>(), expiration);
                    return Enumerable.Empty<Type>();
                }

                if (attr is AuthorizeAttribute authAttribute)
                {
                    foundPolicies ??= new List<Type>();
                    foundPolicies.AddRange(authAttribute.PolicyTypes);
                }
            }
            IEnumerable<Type> results = foundPolicies?.Distinct()?.ToArray();
            this.PolicyCache.AddOrReplace(pageType, results, expiration);
            return results;
        }
    }
}
