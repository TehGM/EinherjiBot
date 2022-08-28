﻿namespace TehGM.EinherjiBot.BotStatus
{
    public interface IStatusStore
    {
        Task<BotStatus> GetAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IEnumerable<BotStatus>> GetAllAsync(CancellationToken cancellationToken = default);

        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

        Task UpsertAsync(BotStatus status, CancellationToken cancellationToken = default);
    }
}
