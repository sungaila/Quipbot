using System;
using System.Threading;
using System.Threading.Tasks;

namespace Quipbot
{
    public abstract class PlayerBase<TObserver, TBehavior, TBrowserContainer> : IPlayer
        where TObserver : IGameObserver, new()
        where TBehavior : IPlayerBehavior, new()
        where TBrowserContainer : IBrowserContainer, new()
    {
        public string Name { get; set; }

        public IGameObserver Observer { get; }

        public IPlayerBehavior Behavior { get; }

        public IBrowserContainer BrowserContainer { get; }

        public PlayerBase() : this(Guid.NewGuid().ToString().Substring(0, 12)) { }

        public PlayerBase(string name) : this(name, new TObserver(), new TBehavior(), new TBrowserContainer()) { }

        public PlayerBase(string name, TObserver observer, TBehavior behavior, TBrowserContainer browserContainer)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            if (name.Length == 0)
                throw new ArgumentOutOfRangeException(nameof(name), "Name cannot be empty.");

            if (name.Length > 12)
                throw new ArgumentOutOfRangeException(nameof(name), "Name cannot be more than 12 characters.");

            Name = name;
            Observer = observer;
            Behavior = behavior;
            BrowserContainer = browserContainer;
        }

        public async Task InitAsync()
        {
            BrowserContainer.Name = Name;

            await Task.WhenAll(Behavior.InitAsync(), BrowserContainer.InitAsync());
            await Task.Delay(BrowserContainer.PollingRate);

            Observer.UpdateState(await BrowserContainer.GetHtmlAsync());
        }

        public async Task SignInAsync(string roomCode)
        {
            if (roomCode == null)
                throw new ArgumentNullException(nameof(roomCode));

            if (roomCode.Length != 4)
                throw new ArgumentOutOfRangeException(nameof(roomCode), "The room code must be 4 characters long.");

            // set room code
            await BrowserContainer.ExecuteScriptAsync($"document.getElementById('roomcode').value = '{roomCode}'; document.getElementById('roomcode').dispatchEvent(new Event('input', {{ 'bubbles': true }}));");
            // set player name
            await BrowserContainer.ExecuteScriptAsync($"document.getElementById('username').value = '{Name}'; document.getElementById('username').dispatchEvent(new Event('input', {{ 'bubbles': true }}));");

            // update game state
            Observer.UpdateState(await BrowserContainer.GetHtmlAsync());

            while (Observer.PageState == PageState.SignInDisabled)
            {
                await Task.Delay(BrowserContainer.PollingRate);
                Observer.UpdateState(await BrowserContainer.GetHtmlAsync());
            }

            // click join button
            await BrowserContainer.ExecuteScriptAsync("document.getElementById('button-join').click()");

            while (Observer.PageState == PageState.SignIn)
            {
                await Task.Delay(BrowserContainer.PollingRate);
                Observer.UpdateState(await BrowserContainer.GetHtmlAsync());
            }

            if (Observer.PageState == PageState.SignInFailed)
                throw new ArgumentException($"The room for the given code '{roomCode.ToUpper()}' was not found.", nameof(roomCode));
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested && BrowserContainer.IsRunning)
            {
                Observer.UpdateState(await BrowserContainer.GetHtmlAsync());
                var script = await Behavior.ReactAsync(Observer);

                if (script != null)
                    await BrowserContainer.ExecuteScriptAsync(script);

                await Task.Delay(BrowserContainer.PollingRate, cancellationToken);
            }
        }

        #region IDisposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    BrowserContainer.Dispose();
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
