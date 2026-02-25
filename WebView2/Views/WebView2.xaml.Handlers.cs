using System.Windows;
using System.Windows.Input;

namespace WebView2Browser
{
    public partial class MainWindow
    {
        private async void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitialized && WebViewControl.CoreWebView2.CanGoBack)
                NavigationHandler.GoBack();
        }

        private async void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitialized && WebViewControl.CoreWebView2.CanGoForward)
                NavigationHandler.GoForward();
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
                NavigationHandler.Refresh();
        }

        private async void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
                await NavigationHandler.NavigateToAddressAsync("about:blank");
        }

        private async void GoButton_Click(object sender, RoutedEventArgs e)
        {
            await NavigationHandler.NavigateToAddressAsync();
        }

        private async void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await NavigationHandler.NavigateToAddressAsync();
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isInitialized)
            {
                WebViewControl.CoreWebView2.Stop();
                StatusText.Text = "Loading stopped";
            }
        }

        private async void DevToolsButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            try
            {
                if (DevToolsWindow == null || !DevToolsWindow.IsLoaded)
                {
                    var environment = await WebViewEnvironment.GetSharedEnvironmentAsync();
                    DevToolsWindow = new DevToolsWindow(WebViewControl.CoreWebView2, environment);
                    DevToolsWindow.Closed += (s, args) => DevToolsWindow = null;
                    DevToolsWindow.Show();
                }
                else
                {
                    DevToolsWindow.Activate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open DevTools: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToggleImages_Changed(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            ImageToggleHandler.ToggleImages();
            ToggleImagesButton.Content = ToggleImagesButton.IsChecked == true ? "Images On" : "Images Off";
        }

        private void ToggleVideos_Changed(object sender, RoutedEventArgs e)
        {
            if (!_isInitialized) return;
            ImageToggleHandler.ToggleVideos();
            // FIX: Changed from ToggleImagesButton to ToggleVideosButton
            ToggleVideosButton.Content = ToggleVideosButton.IsChecked == true ? "Videos On" : "Videos Off";
        }

        private async void FindNext_Click(object sender, RoutedEventArgs e)
        {
            await FindAsync("next", FindBox.Text);
        }

        private async void FindPrev_Click(object sender, RoutedEventArgs e)
        {
            await FindAsync("prev", FindBox.Text);
        }

        private async void FindBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                await FindAsync("start", FindBox.Text);
        }

        private async Task FindAsync(string command, string term = null)
        {
            if (WebViewControl?.CoreWebView2 == null) return;
            try
            {
                var message = new
                {
                    command = command,
                    term = term,
                    caseSensitive = CaseSensitiveChk?.IsChecked == true
                };
                string jsonMessage = System.Text.Json.JsonSerializer.Serialize(message);
                WebViewControl.CoreWebView2.PostWebMessageAsString(jsonMessage);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"FindAsync error: {ex.Message}");
            }
        }
    }
}
