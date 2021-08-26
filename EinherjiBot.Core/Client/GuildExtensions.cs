using Discord;
using Discord.WebSocket;
using DSharpPlus.Entities;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot
{
    public static class GuildExtensions
    {
        public static async Task<SocketGuildUser> GetGuildUserAsync(this SocketGuild guild, ulong id)
        {
            SocketGuildUser user = guild.GetUser(id);
            if (user == null)
            {
                await guild.DownloadUsersAsync();
                user = guild.GetUser(id);
            }
            return user;
        }
        public static Task<SocketGuildUser> GetGuildUserAsync(this SocketGuildChannel channel, ulong id)
            => GetGuildUserAsync(channel.Guild, id);
        public static Task<SocketGuildUser> GetGuildUserAsync(this SocketGuildChannel channel, IUser user)
            => GetGuildUserAsync(channel, user.Id);
        public static Task<SocketGuildUser> GetGuildUserAsync(this SocketGuild guild, IUser user)
            => GetGuildUserAsync(guild, user.Id);

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
