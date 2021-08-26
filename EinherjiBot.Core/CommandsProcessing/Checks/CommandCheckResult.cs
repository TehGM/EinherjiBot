using System;

namespace TehGM.EinherjiBot.CommandsProcessing.Checks
{
    public class CommandCheckResult
    {
        public static CommandCheckResult Success { get; } = new CommandCheckResult(CommandCheckResultType.Continue, null, null);

        public CommandCheckResultType ResultType { get; }
        public string Message { get; }
        public Exception Error { get; }

        public CommandCheckResult(CommandCheckResultType type, string message, Exception error)
        {
            this.ResultType = type;
            this.Message = message;
            this.Error = error;
        }

        public static CommandCheckResult Skip(string message = null)
            => new CommandCheckResult(CommandCheckResultType.Skip, message, null);

        public static CommandCheckResult Abort(string message = null)
            => new CommandCheckResult(CommandCheckResultType.Abort, message, null);

        public static CommandCheckResult Abort(Exception exception, string message = null)
            => new CommandCheckResult(CommandCheckResultType.Abort, message, exception);
    }

    public enum CommandCheckResultType
    {
        /// <summary>Check passed, continue to next or run the command.</summary>
        Continue,
        /// <summary>Check failed, attempt running another command.</summary>
        Skip,
        /// <summary>Check failed, abort command execution completely.</summary>
        Abort
    }
}
