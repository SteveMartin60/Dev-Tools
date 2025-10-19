// WebView2.xaml.cs — Add WebMessageReceived handler

using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WebView2Browser
{
    public partial class MainWindow : Window
    {
        private string _findScript;
        public ICommand OpenDevToolsCommand { get; }

        private WebViewNavigationHandler NavigationHandler { get; set; }
        private DevToolsWindow DevToolsWindow { get; set; }
        private ImageToggleHandler ImageToggleHandler { get; set; }
        private HtmlCaptureService HtmlCaptureService { get; set; }
        private bool _isInitialized { get; set; } = false;
        private string CurrentAddress { get; set; }
        private string SiteAddress { get; set; }
        private string LinksSavePath { get; set; } = @"D:\TS-RO\";
        private string BaseUrl { get; set; }
        public int MaxPageCount { get; set; } = 0;
        public int CurrentPageIndex { get; set; } = 1;

        private List<string> Links { get; set; } = new List<string>();

        private AdBlocker AdBlocker { get; set; }
        private HtmlCaptureManager CaptureManager { get; set; }
        private HtmlCaptureService CaptureService { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            InitializeAsync();

            OpenDevToolsCommand = new RelayCommand(_ => DevToolsButton_Click(this, null));

            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            AddressBar.KeyDown += AddressBar_KeyDown;
            ToggleImagesButton.Checked += ToggleImages_Changed;
            ToggleImagesButton.Unchecked += ToggleImages_Changed;
            ToggleVideosButton.Checked += ToggleVideos_Changed;
            ToggleVideosButton.Unchecked += ToggleVideos_Changed;
        }

        private async void InitializeAsync()
        {
            try
            {
                StatusText.Text = "Initializing WebView2 environment…";
                var environment = await WebViewEnvironment.GetSharedEnvironmentAsync();

                await WebViewControl.EnsureCoreWebView2Async(environment);

                // ────────────────────────────────────────────────────────────────
                // 1. Navigation handler
                // ────────────────────────────────────────────────────────────────
                NavigationHandler = new WebViewNavigationHandler(
                    WebViewControl.CoreWebView2,
                    StatusText,
                    AddressBar);
                NavigationHandler.SetupNavigationHandlers();

                // ────────────────────────────────────────────────────────────────
                // 2. Security / privacy tweaks
                // ────────────────────────────────────────────────────────────────
                WebViewSecuritySettings.ApplySecureSettings(WebViewControl.CoreWebView2.Settings);

                // 3. Custom UA string
                WebViewControl.CoreWebView2.Settings.UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                    "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

                // 4. Deny geolocation
                WebViewControl.CoreWebView2.PermissionRequested += (s, e) =>
                {
                    if (e.PermissionKind == CoreWebView2PermissionKind.Geolocation)
                    {
                        e.State = CoreWebView2PermissionState.Deny;
                        e.Handled = true;
                    }
                };

                // ────────────────────────────────────────────────────────────────
                // 5. Ad-blocking
                // ────────────────────────────────────────────────────────────────
                AdBlocker = new AdBlocker(WebViewControl.CoreWebView2);

                // ────────────────────────────────────────────────────────────────
                // 6. Image / video toggle handler
                // ────────────────────────────────────────────────────────────────
                ImageToggleHandler = new ImageToggleHandler(WebViewControl.CoreWebView2);

                // ────────────────────────────────────────────────────────────────
                // 7. HTML-capture services
                // ────────────────────────────────────────────────────────────────
                HtmlCaptureService = new HtmlCaptureService(WebViewControl.CoreWebView2, StatusText);
                CaptureManager = new HtmlCaptureManager(HtmlCaptureService, NavigationHandler, LinksSavePath);

                // ────────────────────────────────────────────────────────────────
                // 8. Inject find-helper script for Ctrl+F
                // ────────────────────────────────────────────────────────────────
                string findHelperJsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "findHelper.js");
                if (File.Exists(findHelperJsPath))
                {
                    string findScript = File.ReadAllText(findHelperJsPath);
                    await WebViewControl.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(findScript);
                }

                // ────────────────────────────────────────────────────────────────
                // 9. Listen for find results
                // ────────────────────────────────────────────────────────────────
                WebViewControl.CoreWebView2.WebMessageReceived += (sender, args) =>
                {
                    try
                    {
                        string json = args.TryGetWebMessageAsString();
                        using JsonDocument doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        if (root.TryGetProperty("type", out var type) && type.GetString() == "findResult")
                        {
                            int matchCount = root.TryGetProperty("matchCount", out var mc) ? mc.GetInt32() : 0;
                            int activeIndex = -1;

                            if (root.TryGetProperty("activeIndex", out var ai))
                            {
                                activeIndex = ai.GetInt32();
                            }

                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (matchCount == 0)
                                {
                                    MatchLabel.Text = "";
                                }
                                else
                                {
                                    MatchLabel.Text = $"{activeIndex + 1} of {matchCount}";
                                }
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"WebMessageReceived error: {ex.Message}");
                    }
                };

                // ────────────────────────────────────────────────────────────────
                // 10. Ready flag
                // ────────────────────────────────────────────────────────────────
                _isInitialized = true;
                StatusText.Text = "Ready";
            }
            catch (Exception ex)
            {
                StatusText.Text = "Initialization failed";
                MessageBox.Show(
                    $"Failed to initialize WebView2:\n{ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void SetSiteAddress()
        {
            string url = CurrentAddress;
            if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
            {
                SiteAddress = $"{uri.Scheme}://{uri.Host}";
            }
        }

        private void ToggleImages_Changed(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            // Pass the current IsChecked state (true = images enabled, false = images disabled)
            // The handler name 'Changed' implies setting the state based on the checkbox.
            // Assuming the checkbox 'On' means 'images are enabled/displayed'.
            bool imagesEnabled = ToggleImagesButton.IsChecked == true;
            ImageToggleHandler.ToggleImages();
        }

        private void ToggleVideos_Changed(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            // Pass the current IsChecked state (true = videos enabled, false = videos disabled)
            bool videosEnabled = ToggleVideosButton.IsChecked == true;
            ImageToggleHandler.ToggleVideos();
        }


        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            while (!_isInitialized)
                await Task.Delay(50);

            Loaded -= MainWindow_Loaded;

            Activate();
            AddressBar.Focus();
            AddressBar.SelectAll();
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            try
            {
                WebViewControl?.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during cleanup: {ex.Message}", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void CaptureFullHtmlButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;

            try
            {
                string html = await WebViewControl.CoreWebView2.ExecuteScriptAsync(
                    "document.documentElement.outerHTML");

                if (html.StartsWith("\"") && html.EndsWith("\""))
                {
                    html = html.Substring(1, html.Length - 2);
                    html = System.Text.RegularExpressions.Regex.Unescape(html);
                }

                string filePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    $"FullHtmlCapture_{DateTime.Now:yyyyMMddHHmmss}.html");

                File.WriteAllText(filePath, html, Encoding.UTF8);

                MessageBox.Show($"Full HTML saved to:\n{filePath}", "Capture Complete",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to capture HTML: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private static bool IsFocusInside(FrameworkElement container)
        {
            var fe = Keyboard.FocusedElement as DependencyObject;
            if (fe == null) return false;

            // Fast path: focused element *is* the WebView2 control (HwndHost)
            if (fe == container) return true;

            // Walk up the visual tree to see if the container is an ancestor
            for (var parent = fe; parent != null; parent = VisualTreeHelper.GetParent(parent))
                if (ReferenceEquals(parent, container))
                    return true;

            return false;
        }
    }
}
