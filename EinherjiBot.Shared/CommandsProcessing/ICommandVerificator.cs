using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace TehGM.EinherjiBot.CommandsProcessing
{
    public interface ICommandVerificator
    {
        bool Verify(SocketCommandContext command, out string actualCommand);
    }
}
