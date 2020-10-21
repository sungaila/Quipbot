using System;
using System.Threading.Tasks;

namespace Quipbot
{
    public class DummyBrowserContainer : IBrowserContainer
    {
        public string Name { get; set; } = string.Empty;

        public string Website { get; set; } = string.Empty;

        public bool Visible { get; set; }

        public TimeSpan PollingRate { get; set; }

        public bool IsRunning => true;

        public void Dispose()
        {

        }

        public async Task<string> ExecuteScriptAsync(string javaScript)
        {
            return await Task.Run(() => string.Empty);
        }

        public async Task<string> GetHtmlAsync()
        {
            return await Task.Run(() => string.Empty);
        }

        public async Task InitAsync()
        {
            await Task.Delay(0);
        }
    }
}
