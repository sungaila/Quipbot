using System;
using System.Threading;
using System.Threading.Tasks;

namespace Quipbot
{
    public interface IGameManager : IDisposable
    {
        int MinPlayerCount { get; }

        int MaxPlayerCount { get; }

        Task SetupAsync(string roomCode, int playerCount);

        Task RunAsync(CancellationToken cancellationToken = default);
    }
}