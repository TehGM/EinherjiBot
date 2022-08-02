using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using TehGM.EinherjiBot.Security.Authorization;

namespace TehGM.EinherjiBot.UI.Security.Components
{
    public class PolicyAuthorizeView : AuthorizeView
    {
        private AuthenticationState _currentAuthenticationState;
        private bool _isAuthorized;

        [Parameter]
        public Type PolicyType { get; set; }
        [Parameter]
        public IEnumerable<Type> PolicyTypes { get; set; }

        // for route view
        [Parameter]
        public RouteData RouteData { get; set; } = default!;

        [Inject]
        private IDiscordAuthorizationService AuthService { get; set; }
        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        private IEnumerable<Type> _allPolicies;

        private IEnumerable<Type> GetPolicies()
        {
            if (this.PolicyType == null)
                return this.PolicyTypes?.Where(p => p is not null) ?? Enumerable.Empty<Type>();
            return new[] { this.PolicyType }
                .Union(this.PolicyTypes ?? Enumerable.Empty<Type>())
                .Where(p => p is not null);
        }

        protected override async Task OnParametersSetAsync()
        {
            this._allPolicies = this.GetPolicies();
            this._currentAuthenticationState = await this.AuthenticationStateTask.ConfigureAwait(false);
            DiscordAuthorizationResult result = await this.AuthService.AuthorizeAsync(this._allPolicies).ConfigureAwait(false);
            this._isAuthorized = result.Succeeded;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this._isAuthorized || this._allPolicies?.Any() != true)
            {
                RenderFragment<AuthenticationState> authorizedContent = base.Authorized ?? base.ChildContent;
                builder.AddContent(1, authorizedContent?.Invoke(this._currentAuthenticationState));
            }
            else if (this._currentAuthenticationState == null)
            {
                builder.AddContent(0, base.Authorizing);
            }
            else
            {
                builder.AddContent(2, base.NotAuthorized?.Invoke(this._currentAuthenticationState));
            }
        }
    }
}
