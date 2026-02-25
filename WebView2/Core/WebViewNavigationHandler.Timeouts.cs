using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WebView2Browser.Core
{
    public partial class WebViewNavigationHandler
    {
        public async Task NavigateToAddressAsync(string address = "", int timeoutMilliseconds = 30000)
        {
            if (_webView == null) return;

            string currentAddress = string.IsNullOrEmpty(address) ? _addressBar.Text.Trim() : address;
            if (string.IsNullOrWhiteSpace(currentAddress))
            {
                await _webView.ExecuteScriptAsync("window.location.href = 'about:blank';");
                return;
            }

            if (!currentAddress.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !currentAddress.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                currentAddress = "https://" + currentAddress;

            CancelNavigation();
            int thisNavId = Interlocked.Increment(ref _currentNavigationId);

            _navigationTcs = new TaskCompletionSource<bool>();
            _domReadyTcs = new TaskCompletionSource<bool>();
            _primaryTimeoutCts = new CancellationTokenSource();
            _secondaryTimeoutCts = new CancellationTokenSource();
            _heartbeatCts = new CancellationTokenSource();

            var navTcs = _navigationTcs; var domTcs = _domReadyTcs;
            var primaryCts = _primaryTimeoutCts; var secondaryCts = _secondaryTimeoutCts;

            try
            {
                primaryCts.Token.Register(() =>
                { if (_currentNavigationId == thisNavId) _ = HandleSoftTimeout(thisNavId); });

                secondaryCts.Token.Register(() =>
                { if (_currentNavigationId == thisNavId) HandleHardTimeout(thisNavId); });

                primaryCts.CancelAfter(timeoutMilliseconds);
                secondaryCts.CancelAfter(timeoutMilliseconds * 2);

                _ = StartHeartbeatMonitoring(_heartbeatCts.Token, thisNavId);
                _webView.Navigate(currentAddress);

                var navigationTask = Task.WhenAll(navTcs.Task, domTcs.Task);
                var timeoutTask = Task.Delay(timeoutMilliseconds, primaryCts.Token);
                var completedTask = await Task.WhenAny(navigationTask, timeoutTask);

                if (completedTask == navigationTask)
                {
                    bool navSuccess = await navTcs.Task;
                    if (!navSuccess) throw new NavigationException("Navigation failed", isTimeout: false);
                }
                else
                {
                    if (timeoutTask.IsCanceled)
                        throw new NavigationException("Navigation cancelled", isCancelled: true);
                    else
                        throw new NavigationException("Navigation timed out", isTimeout: true);
                }
            }
            catch (OperationCanceledException)
            { throw new NavigationException("Navigation cancelled", isCancelled: true); }
            finally
            { if (_currentNavigationId == thisNavId) CleanupTimeouts(); }
        }

        private void CleanupTimeouts()
        {
            try
            {
                _heartbeatCts?.Cancel(); _primaryTimeoutCts?.Cancel(); _secondaryTimeoutCts?.Cancel();
                _heartbeatCts?.Dispose(); _primaryTimeoutCts?.Dispose(); _secondaryTimeoutCts?.Dispose();
            }
            catch { }
            finally
            { _heartbeatCts = null; _primaryTimeoutCts = null; _secondaryTimeoutCts = null; }
        }
    }
}
