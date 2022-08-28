using Discord;
using TehGM.EinherjiBot.API;

namespace TehGM.EinherjiBot.PlaceholdersEngine.Placeholders
{
    [Placeholder("Role", PlaceholderUsage.Any)]
    [Description("Is replaced with name/mention of a specific role.")]
    public class RolePlaceholder
    {
        [PlaceholderProperty("ID", IsRequired = true, IDType = IDType.Role)]
        [DisplayName("Role ID")]
        [Description("ID of the role. <i>Click on the purple button to open the role picker!</i>")]
        public ulong RoleID { get; init; }
        [PlaceholderProperty("Mode")]
        [DisplayName("Display Mode")]
        [Description("Determines how the role will be displayed.")]
        public RoleDisplayMode DisplayMode { get; init; } = RoleDisplayMode.Mention;

        public class RolePlaceholderHandler : PlaceholderHandler<RolePlaceholder>
        {
            private readonly IDiscordEntityInfoProvider _provider;

            public RolePlaceholderHandler(IDiscordEntityInfoProvider provider)
            {
                this._provider = provider;
            }

            protected override async Task<string> GetReplacementAsync(RolePlaceholder placeholder, CancellationToken cancellationToken = default)
            {
                RoleInfoResponse role = await this._provider.GetRoleInfoAsync(placeholder.RoleID, cancellationToken).ConfigureAwait(false);
                if (role == null)
                    throw new PlaceholderConvertException($"Discord role with ID {placeholder.RoleID} not found, or is not visible by {EinherjiInfo.Name}");

                switch (placeholder.DisplayMode)
                {
                    case RoleDisplayMode.Mention:
                        return MentionUtils.MentionRole(placeholder.RoleID);
                    case RoleDisplayMode.Name:
                        return role.Name;
                    default:
                        throw new ArgumentException($"Unsupported display mode {placeholder.DisplayMode}", nameof(placeholder.DisplayMode));
                }
            }
        }
    }
}
