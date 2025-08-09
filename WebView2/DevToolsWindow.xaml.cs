using Microsoft.Web.WebView2.Core;
using System;
using System.Windows;

namespace WebView2Browser
{
    public partial class DevToolsWindow : Window
    {
        public DevToolsWindow(CoreWebView2 parentWebView, CoreWebView2Environment environment)
        {
            InitializeComponent();
            InitializeDevTools(parentWebView, environment);
            Closing += DevToolsWindow_Closing;
        }

        private async void InitializeDevTools(CoreWebView2 parentWebView, CoreWebView2Environment environment)
        {
            try
            {
                await DevToolsWebView.EnsureCoreWebView2Async(environment);
                parentWebView.OpenDevToolsWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to initialize DevTools: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void DevToolsWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DevToolsWebView?.Dispose();
        }
    }
}
