namespace TehGM.EinherjiBot.SharedAccounts
{
    public interface ISharedAccountImageProvider
    {
        ValueTask<string> GetAccountImageUrlAsync(SharedAccountType accountType, CancellationToken cancellationToken = default);
    }
}
