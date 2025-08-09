using Lib.Mesh.Logging;
using System;
using System.Collections.Specialized;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace GoogleUrlCleaner
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void InputUrl_TextChanged(object sender, TextChangedEventArgs e)
        {
            string inputText = InputUrl.Text.Trim();

            if (string.IsNullOrWhiteSpace(inputText))
            {
                OriginalUrlDisplay.Text = string.Empty;
                CleanedUrlDisplay.Text = string.Empty;
                return;
            }

            // Process all URLs (single or multiple)
            ProcessUrls(inputText);
        }

        private void ProcessUrls(string inputText)
        {
            var urls = inputText.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var originalResults = new StringBuilder();
            var cleanedResults = new StringBuilder();

            foreach (var url in urls)
            {
                var trimmedUrl = url.Trim();
                if (string.IsNullOrWhiteSpace(trimmedUrl))
                    continue;

                try
                {
                    var result = CleanGoogleUrl(trimmedUrl);
                    originalResults.AppendLine(result.OriginalUrl);
                    cleanedResults.AppendLine(result.CleanedUrl);
                }
                catch (UriFormatException)
                {
                    originalResults.AppendLine(trimmedUrl);
                    cleanedResults.AppendLine("Invalid URL format");
                }
                catch (Exception ex)
                {
                    originalResults.AppendLine(trimmedUrl);
                    cleanedResults.AppendLine($"Error: {ex.Message}");
                }
            }

            OriginalUrlDisplay.Text = originalResults.ToString().TrimEnd();
            CleanedUrlDisplay.Text = cleanedResults.ToString().TrimEnd();
        }

        private (string OriginalUrl, string CleanedUrl) CleanGoogleUrl(string inputUrl)
        {
            if (string.IsNullOrWhiteSpace(inputUrl))
                throw new ArgumentException("URL cannot be empty");

            var uri = new Uri(inputUrl);

            if (!IsValidGoogleUrl(uri))
                return (inputUrl, "Not a valid Google search URL");

            var queryParams = ParseQueryString(uri.Query);

            if (!queryParams.HasKeys())
                return (inputUrl, "No query parameters found");

            string searchTerm = queryParams["q"];

            if (string.IsNullOrWhiteSpace(searchTerm))
                return (inputUrl, "Search term (q parameter) not found");

            var cleanedUriBuilder = new UriBuilder(uri.Scheme, uri.Host)
            {
                Path = "/search",
                Query = $"q={Uri.EscapeDataString(searchTerm)}"
            };

            return (inputUrl, cleanedUriBuilder.ToString());
        }

        private bool IsValidGoogleUrl(Uri uri)
        {
            // Check for any Google domain pattern
            if (!uri.Host.Contains(".google.") && !uri.Host.StartsWith("google."))
                return false;

            // Check for either search or image result paths
            return uri.AbsolutePath.StartsWith("/search", StringComparison.OrdinalIgnoreCase) ||
                   uri.AbsolutePath.StartsWith("/imgres", StringComparison.OrdinalIgnoreCase) ||
                   uri.AbsolutePath.StartsWith("/images", StringComparison.OrdinalIgnoreCase);
        }


        private NameValueCollection ParseQueryString(string query)
        {
            var queryParams = new NameValueCollection();

            if (string.IsNullOrWhiteSpace(query) || !query.StartsWith("?"))
                return queryParams;

            query = query.Substring(1); // Remove leading '?'

            foreach (var pair in query.Split('&'))
            {
                if (string.IsNullOrWhiteSpace(pair))
                    continue;

                int index = pair.IndexOf('=');
                if (index >= 0)
                {
                    string key = Uri.UnescapeDataString(pair[..index]);
                    string value = index < pair.Length - 1
                        ? Uri.UnescapeDataString(pair[(index + 1)..])
                        : string.Empty;
                    queryParams.Add(key, value);
                }
                else
                {
                    queryParams.Add(Uri.UnescapeDataString(pair), string.Empty);
                }
            }

            return queryParams;
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            string cleanedUrl = CleanedUrlDisplay.Text.Trim();

            if (string.IsNullOrWhiteSpace(cleanedUrl) ||
                cleanedUrl.StartsWith("Invalid", StringComparison.OrdinalIgnoreCase) ||
                cleanedUrl.StartsWith("No", StringComparison.OrdinalIgnoreCase) ||
                cleanedUrl.StartsWith("Search term", StringComparison.OrdinalIgnoreCase) ||
                cleanedUrl.StartsWith("Not a valid", StringComparison.OrdinalIgnoreCase) ||
                cleanedUrl.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("There is no valid cleaned URL to copy.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                Clipboard.SetText(cleanedUrl);
                MeshLogger.Log("Copy to clipboard success");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to copy to clipboard: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                MeshLogger.Log($"Copy to clipboard failed: {ex.Message}");
            }
        }
    }
}
