using Microsoft.Web.WebView2.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WebView2Browser.Core
{
    public class NavigationProgressEventArgs : EventArgs
    {
        public string Stage { get; set; }
        public int Percentage { get; set; }
        public string Message { get; set; }
        public bool IsError { get; set; }
    }

    public class NavigationException : Exception
    {
        public NavigationException(string message, bool isTimeout = false,
            bool isStall = false, bool isCancelled = false) : base(message)
        {
            IsTimeout = isTimeout; IsStall = isStall; IsCancelled = isCancelled;
        }
        public bool IsTimeout { get; }
        public bool IsStall { get; }
        public bool IsCancelled { get; }
    }

    public partial class WebViewNavigationHandler : IDisposable
    {
        private readonly CoreWebView2 _webView;
        private readonly TextBlock _statusText;
        private readonly TextBox _addressBar;
        private readonly ProgressBar _progressBar;

        private volatile bool _isNavigating = false;
        private int _currentNavigationId = 0;
        private TaskCompletionSource<bool> _navigationTcs;
        private TaskCompletionSource<bool> _domReadyTcs;
        private CancellationTokenSource _primaryTimeoutCts;
        private CancellationTokenSource _secondaryTimeoutCts;
        private CancellationTokenSource _heartbeatCts;

        public DateTime _navigationStartTime;
        private int _retryCount = 0;
        private const int MaxRetries = 2;

        public event EventHandler<NavigationProgressEventArgs> ProgressChanged;
        public event EventHandler<NavigationException> NavigationFailed;

        public WebViewNavigationHandler(CoreWebView2 webView, TextBlock statusText,
            TextBox addressBar, ProgressBar progressBar = null)
        {
            _webView = webView; _statusText = statusText;
            _addressBar = addressBar; _progressBar = progressBar;
        }

        public void SetupNavigationHandlers()
        {
            _webView.NewWindowRequested += OnNewWindowRequested;
            _webView.NavigationStarting += OnNavigationStarting;
            _webView.SourceChanged += OnSourceChanged;
            _webView.ContentLoading += OnContentLoading;
            _webView.DOMContentLoaded += OnDOMContentLoaded;
            _webView.NavigationCompleted += OnNavigationCompleted;
        }

        private void OnNewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            args.Handled = true;
            Application.Current.Dispatcher.Invoke(async () => await NavigateToAddressAsync(args.Uri));
        }

        private void OnNavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs args)
        {
            _isNavigating = true;
            _navigationStartTime = DateTime.Now;
            ReportProgress("Resolving", 10, $"Resolving {new Uri(args.Uri).Host}...");
            Application.Current.Dispatcher.Invoke(() =>
            {
                _statusText.Text = $"Connecting to {args.Uri}...";
                if (_progressBar != null) _progressBar.IsIndeterminate = true;
            });
        }

        private void OnSourceChanged(object sender, CoreWebView2SourceChangedEventArgs args)
        {
            Application.Current.Dispatcher.Invoke(UpdateAddressBar);
            if (_isNavigating) ReportProgress("Connecting", 30, "Connected, loading content...");
        }

        private void OnContentLoading(object sender, CoreWebView2ContentLoadingEventArgs args)
        {
            ReportProgress("Loading", 50, "Loading page content...");
        }

        private void OnDOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs args)
        {
            ReportProgress("DOMReady", 80, "DOM Ready, finalizing...");
            _domReadyTcs?.TrySetResult(true);
        }

        private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs args)
        {
            _isNavigating = false;
            if (args.IsSuccess)
            {
                ReportProgress("Completed", 100, "Ready");
                _navigationTcs?.TrySetResult(true);
                _retryCount = 0;
            }
            else
            {
                ReportProgress("Failed", 0, $"Failed: {args.WebErrorStatus}", true);
                _navigationTcs?.TrySetResult(false);
                if (args.WebErrorStatus != CoreWebView2WebErrorStatus.OperationCanceled)
                    NavigationFailed?.Invoke(this, new NavigationException($"Navigation failed: {args.WebErrorStatus}"));
            }
            Application.Current.Dispatcher.Invoke(() =>
            {
                _statusText.Text = args.IsSuccess ? "Ready" : $"Failed: {args.WebErrorStatus}";
                if (_progressBar != null)
                {
                    _progressBar.Value = args.IsSuccess ? 100 : 0;
                    _progressBar.IsIndeterminate = false;
                }
                if (!_addressBar.IsKeyboardFocusWithin) UpdateAddressBar();
            });
        }

        private void UpdateAddressBar()
        {
            if (_webView == null || string.IsNullOrEmpty(_webView.Source)) return;
            int selStart = _addressBar.SelectionStart;
            int selLen = _addressBar.SelectionLength;
            _addressBar.Text = _webView.Source;
            _addressBar.SelectionStart = selStart;
            _addressBar.SelectionLength = selLen;
        }

        private void ReportProgress(string stage, int percentage, string message, bool isError = false)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ProgressChanged?.Invoke(this, new NavigationProgressEventArgs
                {
                    Stage = stage,
                    Percentage = percentage,
                    Message = message,
                    IsError = isError
                });
                if (_progressBar != null && !isError)
                    _progressBar.Value = Math.Min(100, Math.Max(0, percentage));
            });
        }

        public void GoBack() { if (_webView?.CanGoBack == true) _webView.GoBack(); }
        public void GoForward() { if (_webView?.CanGoForward == true) _webView.GoForward(); }
        public void Refresh() { _retryCount = 0; _webView?.Reload(); }
        public void Stop() => CancelNavigation();
        private void CancelNavigation()
        {
            _isNavigating = false; _currentNavigationId++; _webView?.Stop();
            _navigationTcs?.TrySetCanceled(); _domReadyTcs?.TrySetCanceled();
            CleanupTimeouts();
        }
        public void Dispose() { CancelNavigation(); }
    }
}
