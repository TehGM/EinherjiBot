using System;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using TehGM.EinherjiBot.Logging;

namespace Microsoft.Extensions.Hosting
{
    public static class LoggingInitializationExtensions
    {
        public static IHostBuilder ConfigureSerilog(this IHostBuilder builder)
            => builder.UseSerilog(ConfigureSerilog, true);

        public static void ConfigureSerilog(HostBuilderContext context, LoggerConfiguration config)
        {
            config.ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext();
            DatadogOptions ddOptions = context.Configuration.GetSection("Serilog")?.GetSection("DataDog")?.Get<DatadogOptions>();
            if (!string.IsNullOrWhiteSpace(ddOptions?.ApiKey))
            {
                config.WriteTo.DatadogLogs(
                    ddOptions.ApiKey,
                    source: ".NET",
                    service: ddOptions.ServiceName ?? "Einherji",
                    host: ddOptions.HostName ?? Environment.MachineName,
                    new string[] {
                                $"env:{(ddOptions.EnvironmentName ?? context.HostingEnvironment.EnvironmentName)}",
                                $"assembly:{(ddOptions.AssemblyName ?? context.HostingEnvironment.ApplicationName)}"
                    },
                    ddOptions.ToDatadogConfiguration(),
                    // no need for debug logs in datadag
                    logLevel: ddOptions.OverrideLogLevel ?? LogEventLevel.Information
                );
            }
        }

        public static void EnableUnhandledExceptionLogging()
        {
            if (Log.Logger != null)
                return;

            // add default logger for errors that happen before host runs
            Log.Logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/unhandled.log",
                fileSizeLimitBytes: 1048576,        // 1MB
                rollOnFileSizeLimit: true,
                retainedFileCountLimit: 5,
                rollingInterval: RollingInterval.Day)
                .CreateLogger();
            // capture unhandled exceptions
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Log.Logger.Error((Exception)e.ExceptionObject, "An exception was unhandled");
                Log.CloseAndFlush();
            }
            catch { }
        }
    }
}
