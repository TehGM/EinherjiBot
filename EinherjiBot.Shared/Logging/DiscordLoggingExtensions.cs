using System;
using System.Collections.Generic;
using Discord;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Context;
using Serilog.Events;
using TehGM.EinherjiBot.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace TehGM.EinherjiBot
{
    public static class DiscordLoggingExtensions
    {
        public static void Log(this ILogger logger, LogMessage message)
        {
            LogLevel level = message.Severity.ToLogLevel();
            if (!logger.IsEnabled(level))
                return;

            using (logger.BeginScope(new Dictionary<string, object>()
            {
                { "Label", $"DiscordNet: {message.Source}" }
            }))
            {
                logger.Log(level, message.Exception, message.Message);
            }
        }

        public static IDisposable UseSource(string source)
            => LogContext.PushProperty("Source", source);
        public static IDisposable UseSource(this ILogger log, string source)
            => LogContext.PushProperty("Source", source);

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
                    service: ddOptions.ServiceName ?? "SubBot-CS",
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
            // add default logger for errors that happen before host runs
            Serilog.Log.Logger = new LoggerConfiguration()
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
                Serilog.Log.Logger.Error((Exception)e.ExceptionObject, "An exception was unhandled");
                Serilog.Log.CloseAndFlush();
            }
            catch { }
        }

        public static LogLevel ToLogLevel(this LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Critical:
                    return LogLevel.Critical;
                case LogSeverity.Error:
                    return LogLevel.Error;
                case LogSeverity.Warning:
                    return LogLevel.Warning;
                case LogSeverity.Info:
                    return LogLevel.Information;
                case LogSeverity.Debug:
                    return LogLevel.Debug;
                case LogSeverity.Verbose:
                    return LogLevel.Trace; ;
                default:
                    throw new ArgumentException($"Unknown severity {severity}", nameof(severity));
            }
        }

        public static LogSeverity ToLogSeverity(this LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Critical:
                    return LogSeverity.Critical;
                case LogLevel.Error:
                    return LogSeverity.Error;
                case LogLevel.Warning:
                    return LogSeverity.Warning;
                case LogLevel.Information:
                    return LogSeverity.Info;
                case LogLevel.Debug:
                    return LogSeverity.Debug;
                case LogLevel.Trace:
                    return LogSeverity.Verbose;
                default:
                    throw new ArgumentException($"Unknown log level {level}", nameof(level));
            }
        }
    }
}
