﻿using System;
using Discord;
using MongoDB.Bson.Serialization.Attributes;

namespace TehGM.EinherjiBot
{
    public class UserData
    {
        [BsonId]
        public ulong ID { get; }
        [BsonElement("onlineStatus")]
        public bool IsOnline { get; private set; }
        [BsonElement("onlineStatusChangeTimestamp")]
        public DateTime? StatusChangeTimeUTC { get; private set; }

        [BsonConstructor(nameof(ID))]
        public UserData(ulong id)
        {
            this.ID = id;
        }

        public bool ChangeStatus(bool isOnline)
        {
            if (this.IsOnline == isOnline && StatusChangeTimeUTC != null)
                return false;
            this.IsOnline = isOnline;
            this.StatusChangeTimeUTC = DateTime.UtcNow;
            return true;
        }

        public bool ChangeStatus(UserStatus status)
        {
            if (status == UserStatus.Offline)
                return ChangeStatus(false);
            else return ChangeStatus(true);
        }
    }
}
