using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using Discord.WebSocket;
using TehGM.EinherjiBot.Database;

namespace TehGM.EinherjiBot.Netflix
{
    public class NetflixAccountOptions
    {
        public string DatabaseCollectionName { get; set; } = MongoOptions.MiscellaneousCollectionName;

        public HashSet<ulong> RetrieveRoleIDs { get; set; } = new HashSet<ulong>();
        public HashSet<ulong> ModUsersIDs { get; set; } = new HashSet<ulong>();
        public HashSet<ulong> AllowedChannelsIDs { get; set; } = new HashSet<ulong>();
        public TimeSpan AutoRemoveDelay { get; set; } = TimeSpan.FromSeconds(300);
        public string ThumbnailURL { get; set; } = "https://historia.org.pl/wp-content/uploads/2018/04/netflix-logo.jpg";

        public bool CanRetrieve(IGuildUser user)
            => RetrieveRoleIDs?.Intersect(user.RoleIds)?.Any() == true;
        public bool CanModify(IUser user)
            => ModUsersIDs?.Contains(user.Id) == true;

        public bool IsChannelAllowed(ulong channelID)
            => AllowedChannelsIDs?.Contains(channelID) == true;
        public bool IsChannelAllowed(IChannel channel)
            => IsChannelAllowed(channel.Id);
    }
}
