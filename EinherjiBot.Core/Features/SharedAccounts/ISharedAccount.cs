namespace TehGM.EinherjiBot.SharedAccounts
{
    public interface ISharedAccount
    {
        Guid ID { get; }
        SharedAccountType AccountType { get; }
        string Login { get; }
        string Password { get; }
        IEnumerable<ulong> AuthorizedUserIDs { get; }
        IEnumerable<ulong> AuthorizedRoleIDs { get; }
        IEnumerable<ulong> ModUserIDs { get; }

        ulong? ModifiedByID { get; }
        DateTime? ModifiedTimestamp { get; }
    }
}
