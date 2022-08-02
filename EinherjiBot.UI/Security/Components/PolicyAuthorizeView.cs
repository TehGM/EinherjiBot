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

        [Inject]
        private IDiscordAuthorizationService AuthService { get; set; }
        [CascadingParameter]
        private Task<AuthenticationState> AuthenticationStateTask { get; set; }

        private IEnumerable<Type> GetPolicies()
        {
            if (this.PolicyType == null)
                return this.PolicyTypes;
            return new[] { this.PolicyType }.Union(this.PolicyTypes ?? Enumerable.Empty<Type>());
        }

        protected override async Task OnParametersSetAsync()
        {
            DiscordAuthorizationResult result = await this.AuthService.AuthorizeAsync(this.GetPolicies()).ConfigureAwait(false);
            this._currentAuthenticationState = await this.AuthenticationStateTask.ConfigureAwait(false);
            this._isAuthorized = result.Succeeded;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this._currentAuthenticationState == null)
            {
                builder.AddContent(0, base.Authorizing);
            }
            else if (this._isAuthorized)
            {
                RenderFragment<AuthenticationState> authorizedContent = base.Authorized ?? base.ChildContent;
                builder.AddContent(1, authorizedContent?.Invoke(this._currentAuthenticationState));
            }
            else
            {
                builder.AddContent(2, base.NotAuthorized?.Invoke(this._currentAuthenticationState));
            }
        }
    }
}
