using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot
{
    public static class DiscordGuildExtensions
    {
        public static async Task<DiscordMember> GetMemberSafeAsync(this DiscordGuild guild, ulong id)
        {
            try
            {
                return await guild.GetMemberAsync(id).ConfigureAwait(false);
            }
            catch (DSharpPlus.Exceptions.NotFoundException)
            {
                return null;
            }
        }
    }
}
