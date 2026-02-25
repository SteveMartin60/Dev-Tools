using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;

namespace WebView2Browser
{
    public partial class MainWindow
    {
        private async void CaptureFullHtmlButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            try
            {
                string html = await WebViewControl.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
                if (html.StartsWith("\"") && html.EndsWith("\""))
                {
                    html = html.Substring(1, html.Length - 2);
                    html = Regex.Unescape(html);
                }
                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    $"FullHtmlCapture_{DateTime.Now:yyyyMMddHHmmss}.html");
                File.WriteAllText(filePath, html, System.Text.Encoding.UTF8);
                MessageBox.Show($"Full HTML saved to:\n{filePath}", "Capture Complete",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to capture HTML: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void CaptureHtmlButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            await CaptureManager.CaptureAndProcessHtml();
        }
        private void OnWebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {
            try
            {
                string json = args.TryGetWebMessageAsString();
                using System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(json);
                var root = doc.RootElement;
                if (root.TryGetProperty("type", out var type) && type.GetString() == "findResult")
                {
                    int matchCount = root.TryGetProperty("matchCount", out var mc) ? mc.GetInt32() : 0;
                    int activeIndex = root.TryGetProperty("activeIndex", out var ai) ? ai.GetInt32() : -1;
                    Application.Current.Dispatcher.Invoke(() =>
                        MatchLabel.Text = matchCount == 0 ? "" : $"{activeIndex + 1} of {matchCount}");
                }
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"WebMessageReceived error: {ex.Message}"); }
        }
    }
}
