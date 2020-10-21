using System;

namespace Quipbot
{
    public interface IPlayerBehavior : IInitializable, IDisposable
    {
        IResultProvider ResultProvider { get; }

        string? React(IGameObserver gameObserver);
    }
}