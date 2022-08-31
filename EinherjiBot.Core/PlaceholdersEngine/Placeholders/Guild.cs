﻿using TehGM.EinherjiBot.API;
using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("Guild", PlaceholderUsage.Admin)]
    [Description("Is replaced with name of a specific guild.")]
    public class GuildPlaceholder
    {
        [PlaceholderProperty("ID", IsRequired = true, IDType = IDType.Guild)]
        [DisplayName("Guild ID")]
        [Description("ID of the guild. <i>Click on the purple button to open the guild picker!</i>")]
        public ulong GuildID { get; init; }

        public class GuildPlaceholderHandler : PlaceholderHandler<GuildPlaceholder>
        {
            private readonly IDiscordEntityInfoProvider _provider;
            private readonly IAuthProvider _auth;

            public GuildPlaceholderHandler(IDiscordEntityInfoProvider provider, IAuthProvider auth)
            {
                this._provider = provider;
                this._auth = auth;
            }

            protected async override Task<string> GetReplacementAsync(GuildPlaceholder placeholder, CancellationToken cancellationToken = default)
            {
                if (!this._auth.User.IsAdmin() && !this._auth.User.IsEinherji())
                    throw new AccessForbiddenException("You're not authorized to use Guild placeholder");

                IDiscordGuildInfo guild = await this._provider.GetGuildInfoAsync(placeholder.GuildID, cancellationToken).ConfigureAwait(false);
                if (guild == null)
                    throw new PlaceholderConvertException($"Discord guild with ID {placeholder.GuildID} not found, or is not visible by {EinherjiInfo.Name}");

                return guild.Name;
            }
        }
    }
}
