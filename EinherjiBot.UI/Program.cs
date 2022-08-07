global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;

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
using TehGM.EinherjiBot.Security.API;
using Microsoft.AspNetCore.Components.WebAssembly.Services;
using MudBlazor.Services;

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
            ConfigureServices(builder.Services, builder.HostEnvironment.Environment);
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

        public static void ConfigureSharedServices(IServiceCollection services, string environment)
        {
            services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.SnackbarVariant = MudBlazor.Variant.Outlined;
                config.SnackbarConfiguration.NewestOnTop = true;
                config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomRight;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.PreventDuplicates = false;
            });
            services.AddAuthorizationCore();
            services.AddAuthShared();

            services.AddScoped<LazyAssemblyLoader>();

            services.AddBlazoredLocalStorage();
            services.Replace(ServiceDescriptor.Scoped<IJsonSerializer, NewtonsoftJsonSerializer>());

            // this will be overriden in ConfigureServices on client-side only
            services.TryAddSingleton<IRenderLocation>(s => new Services.RenderLocationProvider(RenderLocation.Server, environment));
        }

        private static void ConfigureServices(IServiceCollection services, string environment)
        {
            ConfigureSharedServices(services, environment);
            services.Replace(ServiceDescriptor.Singleton<IRenderLocation>(new Services.RenderLocationProvider(RenderLocation.Client, environment)));

            services.AddAuthFrontend();
            services.AddEntityCaching();
            services.AddPlaceholdersEngineFrontend();

            services.AddBotStatusFrontend();
            services.AddSharedAccountsFrontend();
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