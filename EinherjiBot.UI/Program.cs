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
using TehGM.EinherjiBot.Logging;
using Microsoft.Extensions.Hosting;
using Serilog;
using Microsoft.Extensions.Configuration;

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
            LoggingInitializationExtensions.EnableUnhandledExceptionLogging();

            var builder = WebAssemblyHostBuilder.CreateDefault(args);

            // create default http client for config loading
            HttpClient client = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
            builder.Services.AddScoped(sp => client);

            // load appsettings
            await builder.Configuration.AddJsonFileAsync(client, "appsettings.json", optional: false).ConfigureAwait(false);
            await builder.Configuration.AddJsonFileAsync(client, $"appsettings.{builder.HostEnvironment.Environment}.json", optional: true).ConfigureAwait(false);

            await builder.Build().RunAsync();
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