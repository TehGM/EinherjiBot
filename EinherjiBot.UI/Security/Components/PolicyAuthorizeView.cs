using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.UI.Security.Components
{
    public class PolicyAuthorizeView : PolicyAuthorizeViewCore
    {
        [Parameter]
        public Func<Task<BotAuthorizationResult>> AuthorizationDelegate { get; set; }

        protected override async Task<BotAuthorizationResult> PerformAuthorizationAsync(IEnumerable<Type> policies)
        {
            BotAuthorizationResult result = await this.AuthService.AuthorizeAsync(this.AllPolicies);

            if (result.Succeeded && this.AuthorizationDelegate != null)
                result = await this.AuthorizationDelegate.Invoke();

            return result;
        }
    }

    public class ResourceAuthorizeView<TResource> : PolicyAuthorizeViewCore
    {
        [Parameter]
        public Func<TResource, Task<BotAuthorizationResult>> AuthorizationDelegate { get; set; }

        protected override async Task<BotAuthorizationResult> PerformAuthorizationAsync(IEnumerable<Type> policies)
        {
            BotAuthorizationResult result = await this.AuthService.AuthorizeAsync((TResource)this.Resource, this.AllPolicies);

            if (result.Succeeded && this.AuthorizationDelegate != null)
                result = await this.AuthorizationDelegate.Invoke((TResource)this.Resource);

            return result;
        }
    }

    public abstract class PolicyAuthorizeViewCore : AuthorizeView
    {
        [Parameter]
        public Type PolicyType { get; set; }
        [Parameter]
        public IEnumerable<Type> PolicyTypes { get; set; }
        [Parameter]
        public EventCallback AuthorizationSucceeded { get; set; }
        [Parameter]
        public EventCallback<BotAuthorizationResult> AuthorizationFailed { get; set; }

        // for route view
        [Parameter]
        public RouteData RouteData { get; set; }

        [Inject]
        protected IRenderLocation RenderLocation { get; private set; }
        [Inject]
        protected IBotAuthorizationService AuthService { get; private set; }
        [CascadingParameter]
        protected Task<AuthenticationState> AuthenticationStateTask { get; private set; }

        protected AuthenticationState CurrentAuthenticationState { get; private set; }
        protected BotAuthorizationResult AuthorizationResult { get; set; }
        protected IEnumerable<Type> AllPolicies { get; private set; }

        protected bool IsAuthorized => this.AuthorizationResult.Succeeded;

        private IEnumerable<Type> GetPolicies()
        {
            if (this.PolicyType == null)
                return this.PolicyTypes?.Where(p => p is not null) ?? Enumerable.Empty<Type>();
            return new[] { this.PolicyType }
                .Union(this.PolicyTypes ?? Enumerable.Empty<Type>())
                .Where(p => p is not null);
        }

        protected abstract Task<BotAuthorizationResult> PerformAuthorizationAsync(IEnumerable<Type> policies);

        protected override async Task OnParametersSetAsync()
        {
            this.AllPolicies = this.GetPolicies();
            this.CurrentAuthenticationState = await this.AuthenticationStateTask.ConfigureAwait(false);

            if (this.RenderLocation.IsServer)
                this.AuthorizationResult = BotAuthorizationResult.Fail("Pre-rendering does not support authorization");
            else
                this.AuthorizationResult = await this.PerformAuthorizationAsync(this.AllPolicies).ConfigureAwait(false);

            if (this.IsAuthorized)
                await this.AuthorizationSucceeded.InvokeAsync();
            else
                await this.AuthorizationFailed.InvokeAsync(this.AuthorizationResult);
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (this.IsAuthorized)
            {
                RenderFragment<AuthenticationState> authorizedContent = base.Authorized ?? base.ChildContent;
                builder.AddContent(1, authorizedContent?.Invoke(this.CurrentAuthenticationState));
            }
            else if (this.CurrentAuthenticationState == null)
            {
                builder.AddContent(0, base.Authorizing);
            }
            else
            {
                builder.AddContent(2, base.NotAuthorized?.Invoke(this.CurrentAuthenticationState));
            }
        }
    }
}
