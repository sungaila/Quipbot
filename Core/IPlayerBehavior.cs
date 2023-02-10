using System;
using System.Threading.Tasks;

namespace Quipbot
{
    public interface IPlayerBehavior : IInitializable, IDisposable
    {
        IResultProvider ResultProvider { get; }

        Task<string?> ReactAsync(IGameObserver gameObserver);
    }
}