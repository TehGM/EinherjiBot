using TehGM.EinherjiBot.SharedAccounts;
using TehGM.EinherjiBot.SharedAccounts.API;

namespace TehGM.EinherjiBot.UI.SharedAccounts
{
    public class WebSharedAccountImageProvider : ISharedAccountImageProvider
    {
        private readonly ISharedAccountsService _service;
        private readonly ILockProvider _lock;

        private IDictionary<SharedAccountType, string> _images;

        public WebSharedAccountImageProvider(ISharedAccountsService service, ILockProvider<WebSharedAccountImageProvider> lockProvider)
        {
            this._service = service;
            this._lock = lockProvider;
        }

        public async ValueTask<string> GetAccountImageUrlAsync(SharedAccountType accountType, CancellationToken cancellationToken = default)
        {
            await this._lock.WaitAsync(cancellationToken);
            try
            {
                if (this._images == null)
                    this._images = await this._service.GetImagesAsync(cancellationToken).ConfigureAwait(false);
                this._images.TryGetValue(accountType, out string result);
                return result;
            }
            finally
            {
                this._lock.Release();
            }
        }
    }
}
