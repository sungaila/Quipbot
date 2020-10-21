using System;
using System.Threading.Tasks;

namespace Quipbot
{
    public interface IBrowserContainer : IInitializable, IDisposable
    {
        string Name { get; set; }

        string Website { get; set; }

        bool Visible { get; set; }

        TimeSpan PollingRate { get; set; }

        bool IsRunning { get; }

        Task<string> GetHtmlAsync();

        Task<string> ExecuteScriptAsync(string javaScript);
    }
}