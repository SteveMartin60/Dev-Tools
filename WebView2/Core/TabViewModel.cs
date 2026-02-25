// ======================================================================
// TabViewModel.cs
// ======================================================================
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.ComponentModel;
using System.Windows;

namespace WebView2Browser
{
    public sealed class TabViewModel : INotifyPropertyChanged, IDisposable
    {
        public Task WhenReady => _readyTcs.Task;
        private readonly TaskCompletionSource<bool> _readyTcs = new();

        public string Header
        {
            get => _header;
            set { _header = value; OnPropertyChanged(); }
        }
        private string _header = "New tab";

        public string Address
        {
            get => _address;
            set { _address = value; OnPropertyChanged(); }
        }
        private string _address = "about:blank";

        public Microsoft.Web.WebView2.Wpf.WebView2 WebView { get; } = new Microsoft.Web.WebView2.Wpf.WebView2();

        public CoreWebView2 CoreWebView2 => WebView.CoreWebView2;

        public TabViewModel()
        {
            _ = InitAsync();
        }

        private async Task InitAsync()
        {
            try
            {
                await WebView.EnsureCoreWebView2Async(await WebViewEnvironment.GetSharedEnvironmentAsync());

                CoreWebView2.NavigationCompleted += (s, e) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Address = WebView.Source?.ToString() ?? "about:blank";
                        Header = Uri.TryCreate(Address, UriKind.Absolute, out var uri)
                            ? uri.Host.Equals("about:blank", StringComparison.OrdinalIgnoreCase)
                                ? "New tab"
                                : uri.Host
                            : "Unknown";
                    });
                };

                _readyTcs.SetResult(true);
            }
            catch (Exception ex)
            {
                _readyTcs.SetException(ex);
            }
        }

        public void Dispose()
        {
            WebView?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}
