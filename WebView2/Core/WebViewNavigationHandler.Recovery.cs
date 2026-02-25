using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace WebView2Browser.Core
{
    public partial class WebViewNavigationHandler
    {
        private async Task StartHeartbeatMonitoring(CancellationToken token, int expectedNavId)
        {
            try
            {
                while (!token.IsCancellationRequested && _isNavigating && _currentNavigationId == expectedNavId)
                {
                    await Task.Delay(5000, token);
                    if (!_isNavigating || token.IsCancellationRequested || _currentNavigationId != expectedNavId) break;

                    var heartbeatTask = _webView.ExecuteScriptAsync("1");
                    var timeoutTask = Task.Delay(10000);
                    var completed = await Task.WhenAny(heartbeatTask, timeoutTask);

                    if (completed == timeoutTask)
                    {
                        ReportProgress("Stalled", 0, "Page unresponsive, attempting recovery...", true);
                        await HandleStallRecovery(expectedNavId);
                        return;
                    }
                }
            }
            catch (Exception ex) { Debug.WriteLine($"Heartbeat error: {ex.Message}"); }
        }

        private async Task HandleSoftTimeout(int expectedNavId)
        {
            if (!_isNavigating || _currentNavigationId != expectedNavId) return;
            Application.Current.Dispatcher.Invoke(() =>
            {
                _statusText.Text = "Page slow to respond, stopping...";
                ReportProgress("Stalled", 75, "Slow connection, forcing stop...");
            });
            try
            {
                _webView.Stop();
                await Task.Delay(5000);
                if (_isNavigating && _currentNavigationId == expectedNavId)
                    _secondaryTimeoutCts?.Cancel();
            }
            catch (Exception ex) { Debug.WriteLine($"Soft timeout error: {ex.Message}"); }
        }

        private void HandleHardTimeout(int expectedNavId)
        {
            if (!_isNavigating || _currentNavigationId != expectedNavId) return;
            Application.Current.Dispatcher.Invoke(() =>
            {
                _statusText.Text = "Page unresponsive, reloading...";
                ReportProgress("Failed", 0, "Page unresponsive - reloading without cache", true);
            });
            try { _webView.Reload(); }
            catch (Exception ex)
            {
                Debug.WriteLine($"Hard timeout error: {ex.Message}");
                NavigationFailed?.Invoke(this, new NavigationException("Hard timeout - reload failed", isTimeout: true));
            }
        }

        private async Task HandleStallRecovery(int expectedNavId)
        {
            if (_currentNavigationId != expectedNavId) return;
            if (_retryCount >= MaxRetries)
            {
                NavigationFailed?.Invoke(this, new NavigationException("Max retries exceeded", isStall: true));
                return;
            }
            _retryCount++;
            Application.Current.Dispatcher.Invoke(() =>
                _statusText.Text = $"Recovering from stall (attempt {_retryCount}/{MaxRetries})...");
            try
            {
                await _webView.ExecuteScriptAsync(@"
                    window.stop();
                    document.querySelectorAll('video, audio, iframe, img').forEach(el => el.remove());
                ");
                await Task.Delay(1000);
                if (_currentNavigationId == expectedNavId) _webView.Reload();
            }
            catch { if (_currentNavigationId == expectedNavId) _webView.Reload(); }
        }
    }
}
