using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Quipbot.Browsers.WebView2
{
    public class WebView2Container : IBrowserContainer
    {
        public string Name { get; set; }

        public string Website { get; set; }

        public bool Visible { get; set; }

        public TimeSpan PollingRate { get; set; } = TimeSpan.FromMilliseconds(500);

        private Thread AppThread { get; set; }

        private WebView2Window? Window { get; set; }

        private ManualResetEventSlim RunningEvent { get; } = new ManualResetEventSlim();

        private ManualResetEventSlim InitEvent { get; } = new ManualResetEventSlim();

        public WebView2Container() : this("https://jackbox.tv/", null, null, true) { }

        public WebView2Container(string website = "https://jackbox.tv/", string? name = null, TimeSpan? pollingRate = null, bool visible = true)
        {
            Name = name ?? $"{nameof(WebView2Container)} ({Guid.NewGuid()})";

            Website = website;
            if (pollingRate.HasValue)
                PollingRate = pollingRate.Value;

            Visible = visible;

            AppThread = new Thread(RunApplication)
            {
                Name = $"{Name} ({Guid.NewGuid()})",
                IsBackground = true
            };
            AppThread.SetApartmentState(ApartmentState.STA);
            AppThread.Start();

            RunningEvent.Wait();
        }

        private void RunApplication()
        {
            if (Application.Current != null)
            {
                RunningEvent.Set();
            }
            else
            {
                var app = new Application
                {
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };

                RunningEvent.Set();
                app.Run();
            }
        }

        public async Task InitAsync()
        {
            if (Window != null)
                throw new InvalidOperationException("The WebView window has been initialized already.");

            Application.Current!.Dispatcher.Invoke(() =>
            {
                void NavigationCompleted(object? s, CoreWebView2NavigationCompletedEventArgs e)
                {
                    InitEvent.Set();
                    Window.WebView2.NavigationCompleted -= NavigationCompleted;
                }

                Window = new WebView2Window { Title = Name };

                if (!Visible)
                {
                    Window.ShowActivated = false;
                    Window.ShowInTaskbar = false;
                    Window.WindowState = WindowState.Minimized;
                    Window.Visibility = Visibility.Hidden;
                }

                Window.Loaded += async (s, e) =>
                {
                    await Window.WebView2.EnsureCoreWebView2Async(
                        await CoreWebView2Environment.CreateAsync(
                            userDataFolder: Path.Combine(Path.GetTempPath(), "Quipbot", "WebView2UserData", Guid.NewGuid().ToString()),
                            options: new CoreWebView2EnvironmentOptions(additionalBrowserArguments: "-inprivate", language: "en-US")));
                };
                Window.ContentRendered += (s, e) => Window.WebView2.Source = new Uri(Website);
                Window.WebView2.NavigationCompleted += NavigationCompleted;

                Window.Show();

                if (!Visible)
                    Window.Hide();
            });

            await Task.Run(InitEvent.Wait);
        }

        public bool IsRunning
        {
            get => Window != null && Application.Current != null && Application.Current.Dispatcher.Invoke(() => Application.Current.Windows.OfType<Window>().Contains(Window));
        }

        public async Task<string> GetHtmlAsync()
        {
            string? json = await ExecuteScriptAsync("document.body.outerHTML");
            using var jsonDoc = JsonDocument.Parse(json);

            return jsonDoc.RootElement.ToString();
        }

        public async Task<string> ExecuteScriptAsync(string javaScript)
        {
            if (Window == null)
                throw new InvalidOperationException("The WebView window has not been initialized yet.");

            return await Window.Dispatcher.Invoke(async () => await Window.WebView2.CoreWebView2.ExecuteScriptAsync(javaScript));
        }

        #region IDisposable
        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Window?.Dispatcher.Invoke(() => Window.Close());
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
