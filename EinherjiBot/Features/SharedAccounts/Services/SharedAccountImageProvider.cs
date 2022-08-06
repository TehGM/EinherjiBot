namespace TehGM.EinherjiBot.SharedAccounts.Services
{
    public class SharedAccountImageProvider : ISharedAccountImageProvider
    {
        private readonly SharedAccountOptions _options;

        public SharedAccountImageProvider(IOptionsSnapshot<SharedAccountOptions> options)
        {
            this._options = options.Value;
        }

        public ValueTask<string> GetAccountImageUrlAsync(SharedAccountType accountType, CancellationToken cancellationToken = default)
        {
            this._options.ImageURLs.TryGetValue(accountType, out string result);
            return ValueTask.FromResult(result);
        }
    }
}
