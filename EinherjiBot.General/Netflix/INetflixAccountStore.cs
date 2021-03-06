﻿using System.Threading;
using System.Threading.Tasks;

namespace TehGM.EinherjiBot.Netflix
{
    public interface INetflixAccountStore
    {
        Task<NetflixAccount> GetAsync(CancellationToken cancellationToken = default);
        Task SetAsync(NetflixAccount account, CancellationToken cancellationToken = default);
    }
}
