using AngleSharp;
using AngleSharp.Html.Parser;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace WebView2Browser.Services
{
    public partial class HtmlCaptureService
    {
        private readonly CoreWebView2 _webView;
        private readonly TextBlock _statusText;
        public HtmlCaptureService(CoreWebView2 webView, TextBlock statusText)
        {
            _webView = webView; _statusText = statusText;
        }
        public async Task<List<string>> CaptureHtmlToFile()
        {
            try
            {
                _statusText.Text = "Capturing HTML...";
                string jsonEncodedHtml = await _webView.ExecuteScriptAsync("document.documentElement.outerHTML");
                string rawHtml = jsonEncodedHtml;
                if (rawHtml.Length >= 2 && rawHtml[0] == '"' && rawHtml[rawHtml.Length - 1] == '"')
                    rawHtml = rawHtml.Substring(1, rawHtml.Length - 2);
                string unescapedHtml = Regex.Unescape(rawHtml);
                var parser = new HtmlParser();
                var document = parser.ParseDocument(unescapedHtml);
                string formattedHtml = document.ToHtml(new AngleSharp.Html.PrettyMarkupFormatter
                { Indentation = "    ", NewLine = "\n" });
                string cleanHtml = RemoveConsecutiveEmptyLines(formattedHtml);
                Directory.CreateDirectory(@"D:\Dev-Tools\WebView2\captured\");
                string captureTitle = _webView.Source.Split("?")[0].Replace("https://", "").Replace("/", "-");
                string filePath = Path.Combine(@"D:\Dev-Tools\WebView2\captured\",
                    $"{captureTitle}{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.html");
                string latestPath = Path.Combine(@"D:\Dev-Tools\WebView2\captured\latest.html");
                File.WriteAllText(filePath, cleanHtml, Encoding.UTF8);
                File.WriteAllText(latestPath, cleanHtml, Encoding.UTF8);
                _statusText.Text = $"HTML saved to {filePath}";
                var lines = cleanHtml.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None).ToList();
                return lines;
            }
            catch (Exception ex)
            {
                _statusText.Text = "Capture failed";
                MessageBox.Show($"Failed to capture HTML: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<string>();
            }
        }
        public string RemoveConsecutiveEmptyLines(string text)
        {
            return Regex.Replace(text, @"(\r?\n){3,}", "\n");
        }
    }
}
