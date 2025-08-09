using Microsoft.Web.WebView2.Core;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace WebView2Browser
{
    public class WebViewNavigationHandler
    {
        private readonly CoreWebView2 _webView;
        private readonly TextBlock _statusText;
        private readonly TextBox _addressBar;
        private TaskCompletionSource<bool> _navigationTaskCompletionSource;
        private TaskCompletionSource<bool> _domReadyCompletionSource;

        public WebViewNavigationHandler(CoreWebView2 webView, TextBlock statusText, TextBox addressBar)
        {
            _webView = webView;
            _statusText = statusText;
            _addressBar = addressBar;
        }

        public void SetupNavigationHandlers()
        {
            _webView.NewWindowRequested += (sender, args) =>
            {
                args.Handled = true;
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    await NavigateToAddressAsync(args.Uri);
                    _statusText.Text = $"Loading: {args.Uri}";
                });
            };

            _webView.NavigationStarting += (sender, args) =>
                Application.Current.Dispatcher.Invoke(() => _statusText.Text = $"Loading: {args.Uri}");


            _webView.NavigationCompleted += (_, args) =>
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _statusText.Text = args.IsSuccess ? "Ready" : $"Failed: {args.WebErrorStatus}";

                    // Only update the bar if it doesn't already have focus
                    if (!_addressBar.IsKeyboardFocusWithin)
                        UpdateAddressBar();
                });


            _webView.SourceChanged += (sender, args) =>
                Application.Current.Dispatcher.Invoke(UpdateAddressBar);
        }

        public async Task NavigateToAddressAsync(string address = "", int timeoutMilliseconds = 30000)
        {
            if (_webView == null) return;

            string currentAddress = string.IsNullOrEmpty(address)
                ? _addressBar.Text.Trim()
                : address;

            if (string.IsNullOrWhiteSpace(currentAddress))
            {
                await _webView.ExecuteScriptAsync("window.location.href = 'about:blank';");
                return;
            }

            if (!currentAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !currentAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                currentAddress = "https://" + currentAddress;
            }

            void NavigationCompletedHandler(object sender, CoreWebView2NavigationCompletedEventArgs e)
            {
                _webView.NavigationCompleted -= NavigationCompletedHandler;
                _navigationTaskCompletionSource?.TrySetResult(e.IsSuccess);
            }

            async void DOMReadyHandler(object sender, EventArgs e)
            {
                _webView.DOMContentLoaded -= DOMReadyHandler;

                try
                {
                    var isPageComplete = await _webView.ExecuteScriptAsync(@"
                        (function() {
                            if (document.readyState === 'complete') return true;
                            return new Promise(resolve => {
                                const timer = setTimeout(() => resolve(false), 5000);
                                window.addEventListener('load', () => {
                                    clearTimeout(timer);
                                    resolve(true);
                                }, {once: true});
                            });
                        })();
                    ");

                    _domReadyCompletionSource?.TrySetResult(isPageComplete == "true");
                }
                catch (Exception ex)
                {
                    _domReadyCompletionSource?.TrySetException(ex);
                }
            }

            try
            {
                _navigationTaskCompletionSource = new TaskCompletionSource<bool>();
                _domReadyCompletionSource = new TaskCompletionSource<bool>();

                var webViewRef = _webView;

                var cancellationTokenSource = new CancellationTokenSource(timeoutMilliseconds);

                cancellationTokenSource.Token.Register(() =>
                {
                    // 1. Always marshal to the UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // 2. Prevent use after dispose
                        try
                        {
                            _webView.Stop();
                        }
                        catch (InvalidOperationException) { /* already gone */ }
                        catch (COMException) { /* already gone */ }
                    });

                    _navigationTaskCompletionSource?.TrySetCanceled();
                    _domReadyCompletionSource?.TrySetCanceled();
                });

                _webView.NavigationCompleted += NavigationCompletedHandler;
                _webView.DOMContentLoaded += DOMReadyHandler;

                _webView.Navigate(currentAddress);

                await Task.WhenAny(
                    Task.WhenAll(_navigationTaskCompletionSource.Task, _domReadyCompletionSource.Task),
                    Task.Delay(timeoutMilliseconds, cancellationTokenSource.Token)
                );

                if (cancellationTokenSource.IsCancellationRequested)
                {
                    throw new TimeoutException($"Navigation timed out after {timeoutMilliseconds}ms");
                }

                if (!_navigationTaskCompletionSource.Task.Result || !_domReadyCompletionSource.Task.Result)
                {
                    //throw new Exception("Page failed to load completely");
                }
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException($"Navigation timed out after {timeoutMilliseconds}ms");
            }
            catch (Exception ex)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    _statusText.Text = "Navigation error";
                });
                throw;
            }
            finally
            {
                _webView.NavigationCompleted -= NavigationCompletedHandler;
                _webView.DOMContentLoaded -= DOMReadyHandler;
            }
        }

        public void GoBack()
        {
            if (_webView?.CanGoBack == true)
                _webView.GoBack();
        }

        public void GoForward()
        {
            if (_webView?.CanGoForward == true)
                _webView.GoForward();
        }

        public void Refresh()
        {
            _webView?.Reload();
        }

        public void GoHome()
        {
            _webView?.Navigate("about:blank");
        }

        private void UpdateAddressBar()
        {
            if (_webView == null || string.IsNullOrEmpty(_webView.Source)) return;

            int selStart = _addressBar.SelectionStart;
            int selLen   = _addressBar.SelectionLength;

            _addressBar.Text = _webView.Source;

            _addressBar.SelectionStart = selStart;
            _addressBar.SelectionLength = selLen;
        }
    }
}
