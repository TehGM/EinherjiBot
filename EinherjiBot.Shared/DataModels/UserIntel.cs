using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Discord;

namespace TehGM.EinherjiBot.DataModels
{
    public class UserIntel
    {
        [JsonProperty("userId", Required = Required.Always)]
        public ulong UserID { get; }
        [JsonProperty("isOnline")]
        public bool IsOnline { get; private set; }
        [JsonProperty("changeTimeUtc")]
        public DateTime? ChangeTimeUTC { get; private set; }

        public UserIntel(ulong userId)
        {
            this.UserID = userId;
        }

        public bool ChangeState(bool isOnline)
        {
            if (this.IsOnline == isOnline && ChangeTimeUTC != null)
                return false;
            this.IsOnline = isOnline;
            this.ChangeTimeUTC = DateTime.UtcNow;
            return true;
        }

        public bool ChangeState(UserStatus status)
        {
            if (status == UserStatus.Offline)
                return ChangeState(false);
            else return ChangeState(true);
        }
    }
}
