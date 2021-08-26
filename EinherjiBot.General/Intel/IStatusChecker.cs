using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace TehGM.EinherjiBot.Intel
{
    public interface IStatusChecker
    {
        Task<UserStatus?> GetStatusAsync(ulong userID);
    }
}
