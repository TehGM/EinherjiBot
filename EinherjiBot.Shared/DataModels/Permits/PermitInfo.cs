using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

namespace TehGM.EinherjiBot.DataModels.Permits
{
    public abstract class PermitInfo
    {
        [JsonProperty("modifiedUserId")]
        public ulong LastModifiedByID { get; private set; }
        [JsonProperty("modifiedTime")]
        public DateTimeOffset LastModifiedTimeUtc { get; private set; }
        [JsonProperty("retrieveRolesIds", Required = Required.Always)]
        public HashSet<ulong> RetrieveRolesID { get; set; }
        [JsonProperty("modifyUsersIds", Required = Required.Default)]
        public HashSet<ulong> ModifyUsersIDs { get; set; }
        [JsonProperty("allowedChannelsIds", Required = Required.Always)]
        public HashSet<ulong> AllowedChannelsIDs { get; set; }
        [JsonProperty("autoremoveDelaySecs", DefaultValueHandling = DefaultValueHandling.Populate, NullValueHandling = NullValueHandling.Ignore)]
        [DefaultValue(5 * 60 * 1000)]
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

        public void UpdateLastModified(ulong userID)
        {
            if (userID == 0)
                return;

            this.LastModifiedByID = userID;
            this.LastModifiedTimeUtc = DateTimeOffset.UtcNow;
        }

        public virtual EmbedBuilder CreateConfirmationEmbed(IUser lastModifiedUser)
        {
            EmbedBuilder embed = new EmbedBuilder();
            if (this.LastModifiedByID != 0)
            {
                embed.WithTimestamp(this.LastModifiedTimeUtc)
                .WithFooter($"Last modified by {lastModifiedUser.Username}#{lastModifiedUser.Discriminator}", lastModifiedUser.GetAvatarUrl());
            }
            embed.WithColor(0, 255, 0);
            AddFieldsToEmbed(ref embed);
            return embed;
        }

        protected abstract void AddFieldsToEmbed(ref EmbedBuilder embed);
        protected abstract UpdateResult UpdateData(SocketCommandContext message, Match match);

        public UpdateResult Update(SocketCommandContext message, Match match)
        {
            UpdateResult result = UpdateData(message, match);
            if (result.IsSuccess)
                UpdateLastModified(message.User.Id);
            return result;
        }


        public class UpdateResult
        {
            public readonly string Message;
            public readonly bool IsSuccess;

            public UpdateResult(bool isSuccess, string message)
            {
                this.Message = message;
                this.IsSuccess = isSuccess;
            }
        }
    }
}
