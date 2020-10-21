using System;
using System.Threading.Tasks;

namespace Quipbot
{
    public abstract class ResultProviderBase<TBrowserContainer> : IResultProvider
        where TBrowserContainer : IBrowserContainer, new()
    {
        public string Name { get; set; }

        public bool Visible { get; set; }

        protected IBrowserContainer BrowserContainer { get; }

        public ResultProviderBase() : this(new TBrowserContainer()) { }

        public ResultProviderBase(TBrowserContainer browserContainer)
        {
            BrowserContainer = browserContainer;
            Name = GetType().Name;
        }

        public virtual async Task InitAsync()
        {
            BrowserContainer.Name = Name;
            BrowserContainer.Visible = Visible;

            await BrowserContainer.InitAsync();
            await Task.Delay(BrowserContainer.PollingRate);

            await SetupAsync();
        }

        protected abstract Task SetupAsync();

        public abstract Task<string?> ProvideResult(string input, TimeSpan? timeout = null);

        #region IDisposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    BrowserContainer?.Dispose();
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
