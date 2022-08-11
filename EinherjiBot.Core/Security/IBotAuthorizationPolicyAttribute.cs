using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.Security
{
    public interface IBotAuthorizationPolicyAttribute
    {
        IEnumerable<Type> PolicyTypes { get; }
    }
}
