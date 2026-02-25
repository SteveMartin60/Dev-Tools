using Microsoft.Web.WebView2.Core;
using System;
using System.Windows;
using WebView2Browser.Core;

namespace WebView2Browser
{
    public partial class MainWindow
    {
        private void OnNavigationProgress(object sender, NavigationProgressEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                StatusText.Text = e.Message;
                LoadingProgressBar.Value = e.Percentage;
                LoadingProgressBar.IsIndeterminate = (e.Stage == "Resolving" || e.Stage == "Connecting");
                StopButton.IsEnabled = (e.Stage != "Completed" && e.Stage != "Failed" && e.Stage != "Stalled");

                if (e.Stage == "Completed")
                {
                    var elapsed = DateTime.Now - _navigationStartTime;
                    LoadTimeText.Text = $"{elapsed.TotalSeconds:F1}s";
                }
                else if (e.Stage == "Resolving")
                {
                    _navigationStartTime = DateTime.Now;
                    LoadTimeText.Text = "";
                }
            });
        }

        private void OnNavigationFailed(object sender, NavigationException e)
        {
            Application.Current.Dispatcher.Invoke(async () =>
            {
                string message = e.IsStall ? "The page became unresponsive."
                    : e.IsTimeout ? "The page took too long to load." : "Navigation failed.";
                message += "\nWould you like to retry without cache?";

                var result = MessageBox.Show(message, "Navigation Problem",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try { await WebViewControl.CoreWebView2.ExecuteScriptAsync("location.reload(true)"); }
                    catch { NavigationHandler.Refresh(); }
                }
                else { UpdateStatus("Ready", 100); LoadingProgressBar.Value = 0; }
            });
        }

        private void OnProcessFailed(object sender, CoreWebView2ProcessFailedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                string reason = e.Reason switch
                {
                    CoreWebView2ProcessFailedReason.Unresponsive => "unresponsive",
                    CoreWebView2ProcessFailedReason.Crashed => "crashed",
                    CoreWebView2ProcessFailedReason.LaunchFailed => "failed to launch",
                    _ => "failed unexpectedly"
                };
                UpdateStatus($"Renderer process {reason}", 0, true);
                if (e.Reason == CoreWebView2ProcessFailedReason.Unresponsive)
                {
                    MessageBox.Show("The page became completely unresponsive and was terminated.",
                        "Page Unresponsive", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            });
        }
    }
}
