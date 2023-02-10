using Microsoft.Web.WebView2.Core;
using System.Windows;

namespace Quipbot.Browsers.WebView2
{
    public partial class WebView2Window : Window
    {
        public WebView2Window()
        {
            InitializeComponent();
        }

        private void WebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            WebView2.Visibility = Visibility.Visible;
            LoadingIndicator.Visibility = Visibility.Collapsed;
        }
    }
}