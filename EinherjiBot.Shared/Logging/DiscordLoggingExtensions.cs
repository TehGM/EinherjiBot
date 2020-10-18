using System;
using System.Collections.Generic;
using Discord;
using Microsoft.Extensions.Logging;
using Serilog.Context;
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
