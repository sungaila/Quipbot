using System;
using System.Threading;
using System.Threading.Tasks;

namespace Quipbot
{
    public interface IPlayer : IInitializable, IDisposable
    {
        IPlayerBehavior Behavior { get; }

        IGameObserver Observer { get; }

        IBrowserContainer BrowserContainer { get; }

        string Name { get; }

        Task SignInAsync(string roomCode);
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}