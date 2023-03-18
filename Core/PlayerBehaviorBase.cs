using System;
using System.Threading.Tasks;

namespace Quipbot
{
    public abstract class PlayerBehaviorBase<TResultProvider> : IPlayerBehavior
        where TResultProvider : IResultProvider, new()
    {
        public IResultProvider ResultProvider { get; }

        public PlayerBehaviorBase() : this(new TResultProvider()) { }

        public PlayerBehaviorBase(TResultProvider resultProvider)
        {
            ResultProvider = resultProvider;
        }

        public async Task InitAsync()
        {
            await ResultProvider.InitAsync();
        }

        public abstract Task<string?> ReactAsync(IGameObserver gameObserver, IPlayer player);

        #region IDisposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    ResultProvider.Dispose();
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
