using Serilog;

namespace Microsoft.Extensions.Hosting
{
    public static class LoggingConfiguration
    {
        public static void StartLoggingUnhandledExceptions()
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                try
                {
                    Log.Logger.Error((Exception)e.ExceptionObject, "An exception was unhandled");
                    Log.CloseAndFlush();
                }
                catch { }
            };
        }
    }
}
