using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            await Task.CompletedTask;
            return Guid.NewGuid().ToString();
        }

        public override async IAsyncEnumerable<string> ProvideResult(string input, int count, TimeSpan? timeout = null)
        {
            await Task.CompletedTask;

            for (int i = 0; i < count; i++)
            {
                yield return Guid.NewGuid().ToString();
            }
        }
    }
}