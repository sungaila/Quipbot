using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Quipbot
{
    public abstract class GameManagerBase<TPlayer> : IGameManager
        where TPlayer : IPlayer, new()
    {
        public abstract int MinPlayerCount { get; }

        public abstract int MaxPlayerCount { get; }

        protected List<TPlayer> Players { get; } = new List<TPlayer>();

        public async Task SetupAsync(string roomCode, int playerCount)
        {
            if (roomCode == null)
                throw new ArgumentNullException(nameof(roomCode));

            if (playerCount < MinPlayerCount || playerCount > MaxPlayerCount)
                throw new ArgumentOutOfRangeException(nameof(playerCount), $"The player count must be between {MinPlayerCount} and {MaxPlayerCount}.");

            Players.Clear();

            for (int i = 0; i < playerCount; i++)
            {
                Players.Add(new TPlayer());
            }

            await InitPlayersAsync(roomCode);
            await SignInPlayersAsync(roomCode);
        }

        public virtual async Task InitPlayersAsync(string roomCode)
        {
            await Task.WhenAll(Players.Select(p => p.InitAsync()));
        }

        public virtual async Task SignInPlayersAsync(string roomCode)
        {
            foreach (var player in Players)
            {
                await player.SignInAsync(roomCode);
            }
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            await Task.WhenAll(Players.Select(p => p.RunAsync(cancellationToken)));
        }

        #region IDisposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (Players != null)
                    {
                        foreach (var player in Players)
                        {
                            player.Dispose();
                        }
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
