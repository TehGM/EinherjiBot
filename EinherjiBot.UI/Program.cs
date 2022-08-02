global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using TehGM.Utilities;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.Configuration;
using TehGM.EinherjiBot.Utilities;
using Blazored.LocalStorage;
using Blazored.LocalStorage.Serialization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Components.Authorization;
using TehGM.EinherjiBot.UI.API.Services;
using TehGM.EinherjiBot.UI.API;
using TehGM.EinherjiBot.Security.API;
using TehGM.EinherjiBot.Security;
using TehGM.EinherjiBot.UI.Security;
using TehGM.EinherjiBot.UI.Security.Services;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.UI
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // add default logger for errors that happen before host runs
            Log.Logger = new LoggerConfiguration()
                        .MinimumLevel.Error()
                        .WriteTo.BrowserConsole()
                        .CreateLogger();
            LoggingConfiguration.StartLoggingUnhandledExceptions();

            JsonConfiguration.InitializeDefaultSettings();

            WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

            // create default http client for config loading
            HttpClient client = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
            builder.Services.AddScoped(sp => client);

            // load appsettings
            await builder.Configuration.AddJsonFileAsync(client, "appsettings.json", optional: false).ConfigureAwait(false);
            await builder.Configuration.AddJsonFileAsync(client, $"appsettings.{builder.HostEnvironment.Environment}.json", optional: true).ConfigureAwait(false);

            ConfigureOptions(builder.Services, builder.Configuration);
            ConfigureServices(builder.Services);
            ConfigureLogging(builder);

            await builder.Build().RunAsync();
        }

        public static void ConfigurePrerenderingOptions(IServiceCollection services, IConfiguration configuration)
        {
        }

        private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
        {
            ConfigurePrerenderingOptions(services, configuration);

            services.Configure<DiscordAuthOptions>(configuration.GetSection("Discord"));
        }

        public static void ConfigurePrerenderingServices(IServiceCollection services)
        {
            services.AddAuthorizationCore();

            services.AddBlazoredLocalStorage();
            services.Replace(ServiceDescriptor.Scoped<IJsonSerializer, NewtonsoftJsonSerializer>());
            services.AddTransient<IDiscordLoginRedirect, DiscordLoginRedirect>();

            // this will be overriden in ConfigureServices on client-side only
            services.TryAddSingleton<IRenderLocation>(s => new Services.RenderLocationProvider(RenderLocation.Server));
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            ConfigurePrerenderingServices(services);
            services.Replace(ServiceDescriptor.Singleton<IRenderLocation>(new Services.RenderLocationProvider(RenderLocation.Client)));

            services.AddTransient<EinherjiBot.Security.Authorization.IDiscordAuthorizationService, EinherjiBot.Security.Authorization.Services.DiscordAuthorizationService>();
            services.AddScoped<IWebAuthProvider, WebAuthenticationStateProvider>();
            services.AddScoped<IAuthProvider>(services => services.GetRequiredService<IWebAuthProvider>());
            services.AddScoped<AuthenticationStateProvider>(services => (AuthenticationStateProvider)services.GetRequiredService<IWebAuthProvider>());
            services.AddTransient<IRefreshTokenProvider, WebRefreshTokenProvider>();

            services.AddTransient<ApiJwtHttpHandler>();
            services.AddTransient<ApiRefreshTokenHttpHandler>();
            services.AddHttpClient<IApiClient, ApiHttpClient>()
                .AddHttpMessageHandler<ApiRefreshTokenHttpHandler>()
                .AddHttpMessageHandler<ApiJwtHttpHandler>();
            services.AddHttpClient<IAuthService, WebAuthService>();

            services.AddTransient<IUserInfoService, WebUserInfoService>();
            services.AddEntityCaching();
        }

        private static void ConfigureLogging(WebAssemblyHostBuilder builder)
        {
            Serilog.Debugging.SelfLog.Enable(m => Console.Error.WriteLine(m));

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Configuration, "Serilog")
                .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(Log.Logger, true);
        }
    }
}