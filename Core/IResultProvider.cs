using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Quipbot
{
    public interface IResultProvider : IInitializable, IDisposable
    {
        Task<string?> ProvideResult(string input, TimeSpan? timeout = null);

        IAsyncEnumerable<string> ProvideResult(string input, int count, TimeSpan? timeout = null);
    }
}