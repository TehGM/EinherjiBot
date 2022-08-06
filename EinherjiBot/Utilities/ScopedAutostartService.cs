using Microsoft.Extensions.DependencyInjection;

namespace TehGM.EinherjiBot
{
    public class ScopedAutostartService : AutostartService
    {
        private readonly IServiceProvider _services;
        private readonly IServiceScopeFactory _factory;

        public ScopedAutostartService(IServiceProvider services)
        {
            this._services = services ?? throw new ArgumentNullException(nameof(services));
        }

        public ScopedAutostartService(IServiceScopeFactory factory)
        {
            this._factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        protected IServiceScope CreateScope()
            => this._factory?.CreateScope() ?? this._services?.CreateScope();

        protected async Task<IServiceScope> CreateBotUserScopeAsync(CancellationToken cancellationToken = default)
        {
            IServiceScope scope = this.CreateScope();
            IDiscordAuthProvider auth = scope.ServiceProvider.GetRequiredService<IDiscordAuthProvider>();
            auth.User = await auth.GetBotContextAsync(base.CancellationToken).ConfigureAwait(false);
            return scope;
        }
    }
}
