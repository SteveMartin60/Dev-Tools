// EventHandlers.cs
using System;
using System.Diagnostics;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace WebView2Browser
{
    public partial class MainWindow : Window
    {
        // --- Navigation Button Event Handlers ---

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

        private async void GoButton_Click(object sender, RoutedEventArgs e) =>
            await NavigationHandler.NavigateToAddressAsync();

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

        // --- Find Feature Event Handlers ---

        /// <summary>
        /// Handles the click event for the "Find Next" button.
        /// Sends a 'next' command to the find helper script via postMessage.
        /// </summary>
        private async void FindNext_Click(object sender, RoutedEventArgs e)
        {
            await FindAsync("next", FindBox.Text);
        }

        /// <summary>
        /// Handles the click event for the "Find Previous" button.
        /// Sends a 'prev' command to the find helper script via postMessage.
        /// </summary>
        private async void FindPrev_Click(object sender, RoutedEventArgs e)
        {
            await FindAsync("prev", FindBox.Text);
        }

        /// <summary>
        /// Handles the KeyDown event for the Find text box.
        /// If Enter is pressed, sends a 'start' command to initiate the search.
        /// </summary>
        private async void FindBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                await FindAsync("start", FindBox.Text);
            }
        }

        /// <summary>
        /// Sends a command to the find helper JavaScript running in the WebView.
        /// Uses PostWebMessageAsString to communicate across the isolated world boundary.
        /// </summary>
        /// <param name="command">The command to send (e.g., "start", "next", "prev", "clear").</param>
        /// <param name="term">The search term, if applicable (e.g., for "start").</param>
        private async Task FindAsync(string command, string term = null)
        {
            // Ensure the WebView2 core is initialized before sending messages
            if (WebViewControl?.CoreWebView2 == null)
            {
                Debug.WriteLine("FindAsync: CoreWebView2 is not initialized.");
                return;
            }

            try
            {
                // Create the message object to send to JavaScript
                var message = new
                {
                    command = command, // "start", "next", "prev", "clear"
                    term = term,       // Search term (can be null for next/prev/clear)
                    caseSensitive = CaseSensitiveChk?.IsChecked == true // Get checkbox state
                };

                // Serialize the message object to a JSON string
                string jsonMessage = JsonSerializer.Serialize(message);

                // Use PostWebMessageAsString to send the JSON message to the page's main world
                // This is the correct method for sending messages to scripts injected via AddScriptToExecuteOnDocumentCreatedAsync
                WebViewControl.CoreWebView2.PostWebMessageAsString(jsonMessage);

                Debug.WriteLine($"FindAsync: Sent message: {jsonMessage}");
            }
            catch (Exception ex)
            {
                // Log any errors that occur during message sending
                Debug.WriteLine($"FindAsync: Error sending message: {ex.Message}");
                // Optionally, show an error message to the user
                // MessageBox.Show($"Find operation failed: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
