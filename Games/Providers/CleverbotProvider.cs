using Quipbot.Browsers.WebView2;
using System;
using System.Threading.Tasks;
using System.Web;

namespace Quipbot.Providers
{
    public class CleverbotProvider : ResultProviderBase<WebView2Container>
    {
        public CleverbotProvider()
        {
            BrowserContainer.Website = "https://www.cleverbot.com/?nocachejs=yes";
            Visible = true;
        }

        protected override async Task SetupAsync()
        {
            // agree to the TOS
            await BrowserContainer.ExecuteScriptAsync("document.querySelector(\"div[id = 'noteb']\").querySelector(\"input[type = 'submit']\").click()");

            // set the spoken language to english
            await BrowserContainer.ExecuteScriptAsync("cleverbot.asr.setlanguage('en',0)");
        }

        public override async Task<string?> ProvideResult(string input, TimeSpan? timeout = null)
        {
            DateTime hideTimeoutAt = DateTime.UtcNow.Add(TimeSpan.FromSeconds(3));
            DateTime? showTimeoutAt = null;

            if (timeout != null)
                showTimeoutAt = DateTime.UtcNow.Add(timeout.Value);

            //await BrowserContainer.ExecuteScriptAsync("cleverbot.newConversation(true,true)");
            //await Task.Delay(BrowserContainer.PollingRate);

            // write input into chat textbox
            await BrowserContainer.ExecuteScriptAsync($"document.querySelector(\"input[name = 'stimulus']\").value = '{HttpUtility.HtmlEncode(input)}'");
            // and send request
            await BrowserContainer.ExecuteScriptAsync("cleverbot.sendAI()");
            await Task.Delay(BrowserContainer.PollingRate);

            while (await BrowserContainer.ExecuteScriptAsync("document.querySelector(\"span[id = 'snipTextIcon']\")") != "null")
            {
                // wait til the share icon disappears
                // we need a timeout here for when the icon reappeared already
                // and we missed it

                if (DateTime.UtcNow > hideTimeoutAt)
                    break;
            }

            while (await BrowserContainer.ExecuteScriptAsync("document.querySelector(\"span[id = 'snipTextIcon']\")") == "null")
            {
                // wait til the share icon shows up (or timeout)

                if (showTimeoutAt.HasValue && DateTime.UtcNow > showTimeoutAt)
                    return null;
            }

            // retrieve bot answer
            var scriptResult = await BrowserContainer.ExecuteScriptAsync("document.querySelector(\"p[id = 'line1']\").querySelector(\"span[class='bot']\").textContent");
            var decoded = HttpUtility.HtmlDecode(scriptResult);
            return decoded.Trim('\"');
        }
    }
}
