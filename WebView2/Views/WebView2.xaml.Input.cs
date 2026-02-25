using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace WebView2Browser
{
    public partial class MainWindow
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (IsFocusInside(WebViewControl))
            {
                if (e.Key == Key.Home || e.Key == Key.End) return;
            }
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.T: e.Handled = true; return; // AddNewTab();
                    case Key.W: case Key.F4: e.Handled = true; return; // CloseCurrentTab();
                    case Key.Tab: e.Handled = true; return; // SelectNextTab(+1);
                    case Key.L: case Key.E: e.Handled = true; FocusAddressBar(); return;
                    case Key.Enter: e.Handled = true; AutoCompleteUrl(); return;
                    case Key.R: e.Handled = true; WebViewControl.CoreWebView2?.Reload(); return;
                }
            }
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.Tab)
            { e.Handled = true; return; } // SelectNextTab(-1);
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            { e.Handled = true; FindBox.Focus(); FindBox.SelectAll(); return; }
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.R)
            { e.Handled = true; WebViewControl.CoreWebView2?.Reload(); return; }
            if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Alt)
            { e.Handled = true; FocusAddressBar(); return; }
            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.F5)
            { e.Handled = true; WebViewControl.CoreWebView2?.Reload(); return; }
            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                switch (e.Key)
                {
                    case Key.Left: e.Handled = true; WebViewControl.CoreWebView2?.GoBack(); return;
                    case Key.Right: e.Handled = true; WebViewControl.CoreWebView2?.GoForward(); return;
                }
            }
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.I || e.Key == Key.F12)
            { e.Handled = true; DevToolsButton_Click(this, null); return; }
            base.OnKeyDown(e);
        }
        private void FocusAddressBar() { AddressBar.Focus(); AddressBar.SelectAll(); }
        private void AutoCompleteUrl()
        {
            var text = AddressBar.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;
            if (!text.StartsWith("http")) text = "www." + text + ".com";
            AddressBar.Text = text.StartsWith("http") ? text : "https://" + text;
            NavigationHandler?.NavigateToAddressAsync(text);
        }
        private static bool IsFocusInside(FrameworkElement container)
        {
            var fe = Keyboard.FocusedElement as DependencyObject;
            if (fe == null) return false;
            if (fe == container) return true;
            for (var parent = fe; parent != null; parent = VisualTreeHelper.GetParent(parent))
                if (ReferenceEquals(parent, container)) return true;
            return false;
        }
    }
}
