using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TehGM.EinherjiBot.DataModels
{
    public class NetflixAccountInfo
    {
        [JsonProperty("login")]
        public string Login { get; private set; }
        [JsonProperty("password")]
        public string Password { get; private set; }
        [JsonProperty("modifiedUserId")]
        public ulong LastModifiedByID { get; private set; }
        [JsonProperty("modifiedTime")]
        public DateTimeOffset LastModifiedTimeUtc { get; private set; }
        [JsonProperty("retrieveRolesIds")]
        public HashSet<ulong> RetrieveRolesID { get; set; }
        [JsonProperty("modifyUsersIds")]
        public HashSet<ulong> ModifyUsersIDs { get; set; }
        [JsonProperty("allowedChannelsIds")]
        public HashSet<ulong> AllowedChannelsIDs { get; set; }
        [JsonProperty("autoremoveDelaySecs")]
        private int _autoremoveDelayMs;
        [JsonIgnore]
        public TimeSpan AutoRemoveDelay
        {
            get { return TimeSpan.FromSeconds(_autoremoveDelayMs); }
            set { _autoremoveDelayMs = (int)value.TotalSeconds; }
        }
        [JsonIgnore]
        public bool IsAutoRemoving => _autoremoveDelayMs > 0;

        public bool CanRetrieve(SocketGuildUser user)
            => RetrieveRolesID.Intersect(user.Roles.Select(r => r.Id)).Any();
        public bool CanModify(IUser user)
            => ModifyUsersIDs.Contains(user.Id);

        public bool IsChannelAllowed(ulong channelID)
            => AllowedChannelsIDs.Contains(channelID);
        public bool IsChannelAllowed(IChannel channel)
            => IsChannelAllowed(channel.Id);

        public void SetLogin(string login, ulong modifyUserID)
        {
            this.Login = login;
            UpdateLastModified(modifyUserID);
        }

        public void SetPassword(string password, ulong modifyUserID)
        {
            this.Password = password;
            UpdateLastModified(modifyUserID);
        }

        private void UpdateLastModified(ulong userID)
        {
            if (userID == 0)
                return;

            this.LastModifiedByID = userID;
            this.LastModifiedTimeUtc = DateTimeOffset.UtcNow;
        }
    }
}
