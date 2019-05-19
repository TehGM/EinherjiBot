using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.Extensions
{
    public static class ClientExtensions
    {
        public static async Task<IUser> GetUserAsync(this DiscordSocketClient client, ulong id, CacheMode mode = CacheMode.AllowDownload)
        {
            IUser user = client.GetUser(id);
            if (user == null && mode == CacheMode.AllowDownload)
                return await client.Rest.GetUserAsync(id);
            return user;
        }
    }
}
