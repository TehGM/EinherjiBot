using Blazored.LocalStorage;

namespace TehGM.EinherjiBot.UI.Security.Services
{
    public class WebRefreshTokenProvider : IRefreshTokenProvider
    {
        private const string _refreshTokenStorageKey = "refreshToken";
        private readonly ILocalStorageService _storage;

        public WebRefreshTokenProvider(ILocalStorageService storage)
        {
            this._storage = storage;
        }

        public ValueTask<string> GetAsync(CancellationToken cancellationToken = default)
            => this._storage.GetItemAsync<string>(_refreshTokenStorageKey, cancellationToken);

        public ValueTask SetAsync(string token, CancellationToken cancellationToken = default)
        {
            if (token == null)
                return this.ClearAsync(cancellationToken);
            return this._storage.SetItemAsync<string>(_refreshTokenStorageKey, token, cancellationToken);
        }

        public ValueTask ClearAsync(CancellationToken cancellationToken = default)
            => this._storage.RemoveItemAsync(_refreshTokenStorageKey, cancellationToken);
    }
}
