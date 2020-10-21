using Quipbot.Base;
using Quipbot.Browsers.WebView2;
using System;
using System.Threading.Tasks;
using System.Web;

namespace Quipbot.Providers
{
    public class GuidProvider : ResultProviderBase<DummyBrowserContainer>
    {
        protected override async Task SetupAsync()
        {
            await Task.Delay(0);
        }

        public override async Task<string?> ProvideResult(string input, TimeSpan? timeout = null)
        {
            return await Task.Run(() => Guid.NewGuid().ToString());
        }
    }
}
