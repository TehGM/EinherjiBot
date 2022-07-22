global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text.RegularExpressions;
global using System.Threading;
global using System.Threading.Tasks;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using TehGM.Utilities;
global using TehGM.EinherjiBot.Security;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TehGM.EinherjiBot.Logging;
using TehGM.EinherjiBot.Utilities;

namespace TehGM.EinherjiBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // add default logger for errors that happen before host runs
            Log.Logger = new LoggerConfiguration()
                        .WriteTo.Console()
                        .WriteTo.File("logs/unhandled.log",
                        fileSizeLimitBytes: 1048576,        // 1MB
                        rollOnFileSizeLimit: true,
                        retainedFileCountLimit: 5,
                        rollingInterval: RollingInterval.Day)
                        .CreateLogger();
            LoggingConfiguration.StartLoggingUnhandledExceptions();

            JsonConfiguration.InitializeDefaultSettings();

            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            ConfigureHost(builder.Host);

            ConfigureOptions(builder.Services, builder.Configuration);
            ConfigureServices(builder.Services);

            WebApplication app = builder.Build();
            ConfigureApplication(app);
            app.Run();
        }

        private static void ConfigureHost(IHostBuilder host)
        {
            host.ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsecrets.json", optional: true, reloadOnChange: true);
                config.AddJsonFile($"appsecrets.{hostingContext.HostingEnvironment.EnvironmentName}.json", optional: true, reloadOnChange: true);
            });
            host.UseSerilog(ConfigureSerilog, true);
        }

        private static void ConfigureOptions(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<Database.MongoOptions>(configuration.GetSection("Database"));
            services.Configure<DiscordClient.DiscordOptions>(configuration.GetSection("Discord"));
            services.Configure<RandomStatus.RandomStatusOptions>(configuration.GetSection("RandomStatus"));
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddRazorPages();

            services.AddDiscordClient();
            services.AddEntityCaching();
            services.AddUserContext();
            services.AddBotAudits();
            services.AddRandomizer();

            services.AddUserIntel();
            services.AddAdministration();
            services.AddRandomStatus();
            services.AddGameServers();
        }

        private static void ConfigureApplication(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();


            app.MapRazorPages();
            app.MapControllers();
            app.MapFallbackToPage("/_Host");
        }

        private static void ConfigureSerilog(HostBuilderContext context, LoggerConfiguration logConfig)
        {
            logConfig.ReadFrom.Configuration(context.Configuration, "Serilog");

            DatadogOptions ddOptions = context.Configuration.GetSection("Serilog").GetSection("DataDog").Get<DatadogOptions>();
            if (!string.IsNullOrWhiteSpace(ddOptions?.ApiKey))
            {
                logConfig.WriteTo.DatadogLogs(
                    ddOptions.ApiKey,
                    source: ".NET",
                    service: ddOptions.ServiceName,
                    host: ddOptions.HostName ?? Environment.MachineName,
                    new string[] {
                        $"env:{ddOptions.EnvironmentName ?? context.HostingEnvironment.EnvironmentName}"
                    },
                    ddOptions.ToDatadogConfiguration()
                );
            }
        }
    }
}