using System;
using System.Threading.Tasks;

namespace Quipbot
{
    public interface IResultProvider : IInitializable, IDisposable
    {
        bool Visible { get; set; }

        Task<string?> ProvideResult(string input, TimeSpan? timeout = null);
    }
}