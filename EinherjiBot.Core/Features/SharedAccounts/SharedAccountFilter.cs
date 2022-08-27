using MongoDB.Driver;
using TehGM.EinherjiBot.Security;

namespace TehGM.EinherjiBot.SharedAccounts
{
    public class SharedAccountFilter : IEntityFilter<ISharedAccount>
    {
        public static SharedAccountFilter Empty { get; } = new SharedAccountFilter();

        /// <summary>If not null or empty, only accounts with Login starting with specified phrase will be returned.</summary>
        public string LoginStartsWith { get; set; }
        /// <summary>If not null or empty, only accounts with Login containing specified phrase will be returned.</summary>
        public string LoginContains { get; set; }

        /// <summary>If not null, only accounts of specific type will be returned.</summary>
        public SharedAccountType? AccountType { get; set; }

        /// <summary>If not null, only accounts that have specified user in their ACL will be returned. Ignores roles ACL.</summary>
        public ulong? AuthorizeUserID { get; set; }
        /// <summary>If not null, only accounts that have any of specified roles in their ACL will be returned. Ignores users ACL.</summary>
        public IEnumerable<ulong> AuthorizeRoleIDs { get; set; }
        /// <summary>If not null, only accounts that have specified user in their modlist will be returned.</summary>
        public ulong? ModUserID { get; set; }

        public SharedAccountFilter FilterUserAccess(IAuthContext user)
        {
            if (user.IsAdmin() || user.IsEinherji())
                return this;

            this.AuthorizeUserID = user.ID;
            this.AuthorizeRoleIDs = user.RecognizedDiscordRoleIDs ?? Enumerable.Empty<ulong>();
            return this;
        }

        public static SharedAccountFilter ForUserAccess(IAuthContext user)
            => new SharedAccountFilter().FilterUserAccess(user);

        public bool Check(ISharedAccount entity)
        {
            if (!string.IsNullOrEmpty(this.LoginStartsWith) && !entity.Login.StartsWith(this.LoginStartsWith, StringComparison.OrdinalIgnoreCase))
                return false;
            if (!string.IsNullOrEmpty(this.LoginContains) && !entity.Login.Contains(this.LoginStartsWith, StringComparison.OrdinalIgnoreCase))
                return false;
            if (this.AccountType != null && this.AccountType != entity.AccountType)
                return false;

            // ACL filters need special treatment, as we want to allow by user OR role
            bool authUser = this.AuthorizeUserID == null || (entity.AuthorizedUserIDs != null && !entity.AuthorizedUserIDs.Contains(this.AuthorizeUserID.Value));
            bool authRole = this.AuthorizeRoleIDs == null || (entity.AuthorizedRoleIDs != null && !entity.AuthorizedRoleIDs.Intersect(this.AuthorizeRoleIDs).Any());
            if (!authUser && !authRole)
                return false;

            if (this.ModUserID != null && entity.ModUserIDs?.Contains(this.ModUserID.Value) != true)
                return false;

            return true;
        }
    }
}
