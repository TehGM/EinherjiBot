using System;
using Discord;
using Serilog;
using Serilog.Context;
using Serilog.Events;

namespace TehGM.EinherjiBot.Utilities
{
    public static class Logging
    {
        public static ILogger Default
        {
            get
            {
                if (Log.Logger == null)
                    Log.Logger = CreateDefaultConfiguration().CreateLogger();
                return Log.Logger;
            }
            set => Log.Logger = value;
        }

        public static IDisposable UseSource(string source)
            => LogContext.PushProperty("Source", source);

        public static LoggerConfiguration CreateDefaultConfiguration()
        {
            string format = "[{Timestamp:HH:mm:ss} {Level}] {Source} {Message:lj}{NewLine}{Exception}";
            return new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.Console(outputTemplate: format)
                .WriteTo.Async(to => to.File("logs/bot.log",
                    fileSizeLimitBytes: 1048576,        // 10MB
                    rollOnFileSizeLimit: true,
                    retainedFileCountLimit: 10,
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: format));
        }

        public static void HandleDiscordNetLog(LogMessage logMessage)
        {
            LogEventLevel level = SeverityToSerilogLevel(logMessage.Severity);
            using (UseSource(logMessage.Source))
                Default.Write(level, logMessage.Exception, logMessage.Message);
        }

        public static LogEventLevel SeverityToSerilogLevel(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Critical:
                    return LogEventLevel.Fatal;
                case LogSeverity.Error:
                    return LogEventLevel.Error;
                case LogSeverity.Warning:
                    return LogEventLevel.Warning;
                case LogSeverity.Info:
                    return LogEventLevel.Information;
                case LogSeverity.Debug:
                    return LogEventLevel.Debug;
                case LogSeverity.Verbose:
                    return LogEventLevel.Verbose;
                default:
                    throw new ArgumentException($"Unknown severity {severity}", nameof(severity));
            }
        }

        public static LogSeverity SerilogLevelToSeverity(LogEventLevel level)
        {
            switch (level)
            {
                case LogEventLevel.Fatal:
                    return LogSeverity.Critical;
                case LogEventLevel.Error:
                    return LogSeverity.Error;
                case LogEventLevel.Warning:
                    return LogSeverity.Warning;
                case LogEventLevel.Information:
                    return LogSeverity.Info;
                case LogEventLevel.Debug:
                    return LogSeverity.Debug;
                case LogEventLevel.Verbose:
                    return LogSeverity.Verbose;
                default:
                    throw new ArgumentException($"Unknown log level {level}", nameof(level));
            }
        }
    }
}
