using AngleSharp;
using AngleSharp.Html;
using AngleSharp.Html.Parser;
using HtmlAgilityPack;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace WebView2Browser
{
    public class HtmlCaptureService
    {
        private readonly CoreWebView2 _webView;
        private readonly TextBlock _statusText;

        public HtmlCaptureService(CoreWebView2 webView, TextBlock statusText)
        {
            _webView = webView;
            _statusText = statusText;
        }

        public async Task<List<string>> CaptureHtmlToFile()
        {
            try
            {
                _statusText.Text = "Capturing HTML...";

                // Get raw HTML from WebView2
                string jsonEncodedHtml = await _webView.ExecuteScriptAsync(
                    "document.documentElement.outerHTML");

                // Remove JSON quotes
                string rawHtml = jsonEncodedHtml;
                if (rawHtml.Length >= 2 && rawHtml[0] == '"' && rawHtml[rawHtml.Length - 1] == '"')
                {
                    rawHtml = rawHtml.Substring(1, rawHtml.Length - 2);
                }

                // Unescape characters
                string unescapedHtml = Regex.Unescape(rawHtml);

                // Parse and format with AngleSharp
                var parser = new HtmlParser();
                var document = parser.ParseDocument(unescapedHtml);

                string formattedHtml = document.ToHtml(new AngleSharp.Html.PrettyMarkupFormatter
                {
                    Indentation = "    ", // 4 spaces
                    NewLine = "\n"
                });

                // Clean up empty lines (keeping single empty lines)
                string cleanHtml = RemoveConsecutiveEmptyLines(formattedHtml);

                // Ensure directory exists
                Directory.CreateDirectory(@"D:\Dev-Tools\WebView2\captured\");                

                string CaptureTitle = _webView.Source.Split("?")[0].Replace("https://", "").Replace("/", "-");

                string filePath   = Path.Combine(@"D:\Dev-Tools\WebView2\captured\", $"{CaptureTitle}{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.html");
                string latestPath = Path.Combine(@"D:\Dev-Tools\WebView2\captured\latest.html");

                // Save to file
                File.WriteAllText(filePath,   cleanHtml, Encoding.UTF8);
                File.WriteAllText(latestPath, cleanHtml, Encoding.UTF8);

                _statusText.Text = $"HTML saved to {filePath}";
                //MessageBox.Show($"HTML content saved to:\n{filePath}", "Capture Complete", MessageBoxButton.OK, MessageBoxImage.Information);

                // Split into lines - keeping empty lines but removing consecutive ones
                var lines = cleanHtml.Split(
                    new[] { "\r\n", "\r", "\n" },
                    StringSplitOptions.None
                ).ToList();

                return lines;
            }
            catch (Exception ex)
            {
                _statusText.Text = "Capture failed";
                MessageBox.Show($"Failed to capture HTML: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<string>(); // Return empty list on error
            }
        }

        public string FormatHtmlWithBetterIndentation(string rawHtml)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(rawHtml);

            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                foreach (var node in doc.DocumentNode.ChildNodes)
                {
                    WriteNodeWithImprovedFormatting(node, writer, 0);
                }
            }

            return sb.ToString();
        }

        private void WriteNodeWithImprovedFormatting(HtmlNode node, TextWriter writer, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 4);
            string childIndent = new string(' ', (indentLevel + 1) * 4);

            switch (node.NodeType)
            {
                case HtmlNodeType.Element:
                    // Skip formatting for inline elements and special cases
                    if (IsInlineElement(node.Name) ||
                        node.HasClass("prettier-ignore-start") ||
                        node.HasClass("prettier-ignore-end"))
                    {
                        writer.Write(node.OuterHtml);
                        return;
                    }

                    writer.Write($"{indent}<{node.Name}");

                    // Write attributes
                    if (node.HasAttributes)
                    {
                        foreach (var attr in node.Attributes)
                        {
                            writer.Write($" {attr.Name}=\"{attr.Value}\"");
                        }
                    }

                    if (node.ChildNodes.Count == 0 && HtmlNode.IsEmptyElement(node.Name))
                    {
                        writer.WriteLine(" />");
                    }
                    else
                    {
                        writer.WriteLine(">");

                        // Special handling for text nodes to avoid extra newlines
                        if (node.ChildNodes.Count == 1 &&
                            node.FirstChild.NodeType == HtmlNodeType.Text)
                        {
                            writer.Write(childIndent);
                            WriteNodeWithImprovedFormatting(node.FirstChild, writer, 0);
                            writer.WriteLine();
                        }
                        else
                        {
                            foreach (var child in node.ChildNodes)
                            {
                                WriteNodeWithImprovedFormatting(child, writer, indentLevel + 1);
                            }
                        }

                        writer.WriteLine($"{indent}</{node.Name}>");
                    }
                    break;

                case HtmlNodeType.Text:
                    string text = node.InnerText.Trim();
                    if (!string.IsNullOrEmpty(text))
                    {
                        writer.Write(text);
                    }
                    break;

                case HtmlNodeType.Comment:
                    writer.WriteLine($"{indent}<!--{node.InnerHtml}-->");
                    break;

                default:
                    writer.Write(node.OuterHtml);
                    break;
            }
        }

        private bool IsInlineElement(string tagName)
        {
            return tagName switch
            {
                "a" or "span" or "strong" or "em" or "b" or "i" or "img" or "svg" or "use" => true,
                _ => false
            };
        }

        public string RemoveConsecutiveEmptyLines(string text)
        {
            return Regex.Replace(text, @"(\r?\n){3,}", "\n\n");
        }
    }
}
