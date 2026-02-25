using Microsoft.Web.WebView2.Core;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WebView2Browser.Core;
using WebView2Browser.Services;

namespace WebView2Browser
{
    public partial class MainWindow : Window
    {
        private bool _isInitialized = false;
        private DateTime _navigationStartTime;
        private WebViewNavigationHandler NavigationHandler { get; set; }
        private DevToolsWindow DevToolsWindow { get; set; }
        private ImageToggleHandler ImageToggleHandler { get; set; }
        private AdBlocker AdBlocker { get; set; }
        private HtmlCaptureService HtmlCaptureService { get; set; }
        private HtmlCaptureManager CaptureManager { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            AddressBar.KeyDown += AddressBar_KeyDown;
            ToggleImagesButton.Checked += ToggleImages_Changed;
            ToggleImagesButton.Unchecked += ToggleImages_Changed;
            ToggleVideosButton.Checked += ToggleVideos_Changed;
            ToggleVideosButton.Unchecked += ToggleVideos_Changed;
            InitializeAsync();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            while (!_isInitialized) await Task.Delay(50);
            Loaded -= MainWindow_Loaded;
            Activate(); AddressBar.Focus(); AddressBar.SelectAll();
        }

        private void MainWindow_Closing(object? sender, CancelEventArgs e)
        {
            try { NavigationHandler?.Dispose(); WebViewControl?.Dispose(); }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during cleanup: {ex.Message}", "Warning",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void InitializeAsync()
        {
            try
            {
                UpdateStatus("Initializing WebView2 environment…", 0);
                var environment = await WebViewEnvironment.GetSharedEnvironmentAsync();
                await WebViewControl.EnsureCoreWebView2Async(environment);

                NavigationHandler = new WebViewNavigationHandler(
                    WebViewControl.CoreWebView2, StatusText, AddressBar, LoadingProgressBar);
                NavigationHandler.ProgressChanged += OnNavigationProgress;
                NavigationHandler.NavigationFailed += OnNavigationFailed;
                NavigationHandler.SetupNavigationHandlers();

                WebViewSecuritySettings.ApplySecureSettings(WebViewControl.CoreWebView2.Settings);
                WebViewControl.CoreWebView2.Settings.UserAgent =
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                    "(KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

                WebViewControl.CoreWebView2.PermissionRequested += (s, e) =>
                {
                    if (e.PermissionKind == CoreWebView2PermissionKind.Geolocation)
                    { e.State = CoreWebView2PermissionState.Deny; e.Handled = true; }
                };
                WebViewControl.CoreWebView2.ProcessFailed += OnProcessFailed;

                AdBlocker = new AdBlocker(WebViewControl.CoreWebView2);
                ImageToggleHandler = new ImageToggleHandler(WebViewControl.CoreWebView2);
                HtmlCaptureService = new HtmlCaptureService(WebViewControl.CoreWebView2, StatusText);
                CaptureManager = new HtmlCaptureManager(HtmlCaptureService, NavigationHandler, @"D:\TS-RO\");

                string findHelperJsPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources\\findHelper.js");
                if (System.IO.File.Exists(findHelperJsPath))
                {
                    string findScript = System.IO.File.ReadAllText(findHelperJsPath);
                    await WebViewControl.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(findScript);
                }
                WebViewControl.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
                _isInitialized = true;
                UpdateStatus("Ready", 100);
            }
            catch (Exception ex)
            {
                UpdateStatus("Initialization failed", 0, true);
                MessageBox.Show($"Failed to initialize WebView2:\n{ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateStatus(string message, int progressPercent, bool isError = false)
        {
            StatusText.Text = message; LoadingProgressBar.Value = progressPercent;
            StatusText.Foreground = isError ? System.Windows.Media.Brushes.Red : System.Windows.Media.Brushes.Black;
        }
    }
}
