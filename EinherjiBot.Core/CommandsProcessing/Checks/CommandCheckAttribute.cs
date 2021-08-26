using System;
using System.Threading;
using System.Threading.Tasks;
using TehGM.EinherjiBot.CommandsProcessing.Checks;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public abstract class CommandCheckAttribute : Attribute
    {
        public abstract Task<CommandCheckResult> RunCheckAsync(CommandContext context, IServiceProvider services, CancellationToken cancellationToken = default);
    }
}
