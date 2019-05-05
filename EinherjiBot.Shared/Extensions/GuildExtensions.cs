using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot
{
    public static class GuildExtensions
    {
        public static async Task<SocketGuildUser> GetGuildUser(this SocketGuild guild, ulong id)
        {
            SocketGuildUser user = guild.GetUser(id);
            if (user == null)
            {
                await guild.DownloadUsersAsync();
                user = guild.GetUser(id);
            }
            return user;
        }
        public static Task<SocketGuildUser> GetGuildUser(this SocketGuildChannel channel, ulong id)
            => GetGuildUser(channel.Guild, id);
        public static Task<SocketGuildUser> GetGuildUser(this SocketGuildChannel channel, IUser user)
            => GetGuildUser(channel, user.Id);
        public static Task<SocketGuildUser> GetGuildUser(this SocketGuild guild, IUser user)
            => GetGuildUser(guild, user.Id);
    }
}
