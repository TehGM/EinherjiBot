﻿namespace TehGM.EinherjiBot.Security
{
    public interface IRefreshTokenStore
    {
        Task<RefreshToken> GetAsync(string token, CancellationToken cancellationToken = default);
        Task AddAsync(RefreshToken token, CancellationToken cancellationToken = default);
        Task DeleteAsync(string token, CancellationToken cancellationToken = default);
    }
}
