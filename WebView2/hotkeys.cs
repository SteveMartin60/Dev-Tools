// hotkeys.cs
// Standard Edge-like keyboard shortcuts for WebView2Browser
// ------------------------------------------------------------------

using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using WindowsInput;
using WindowsInput.Native;

namespace WebView2Browser
{
    public partial class MainWindow : Window
    {
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (IsFocusInside(WebViewControl))
            {
                if (e.Key == Key.Home || e.Key == Key.End)
                {
                    // Let Chromium handle caret movement in the HTML input/textarea.
                    // Do NOT call base.OnKeyDown(e); just return.
                    return;
                }
            }
            // -----------------------------------------------------------------
            // TAB MANAGEMENT
            // -----------------------------------------------------------------
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.T:                 // Ctrl + T → New tab
                        e.Handled = true;
                        AddNewTab();            // your existing method
                        return;

                    case Key.W:                 // Ctrl + W → Close tab
                    case Key.F4:                // Ctrl + F4 → Close tab
                        e.Handled = true;
                        CloseCurrentTab();      // your existing method
                        return;

                    case Key.Tab:               // Ctrl + Tab → Next tab
                        e.Handled = true;
                        SelectNextTab(+1);
                        return;
                }
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.Tab)
            {                                   // Ctrl + Shift + Tab → Previous tab
                e.Handled = true;
                SelectNextTab(-1);
                return;
            }

            // -----------------------------------------------------------------
            // ADDRESS BAR & SEARCH
            // -----------------------------------------------------------------
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.L:                 // Ctrl + L → focus address bar
                    case Key.E:                 // Ctrl + E → focus address bar (search mode)
                        e.Handled = true;
                        AddressBar.Focus();
                        AddressBar.SelectAll();
                        return;

                    case Key.Enter:             // Ctrl + Enter → auto-add www./.com
                        e.Handled = true;
                        AutoCompleteUrl();
                        return;

                    case Key.R:                 // Ctrl + R → reload
                        e.Handled = true;
                        WebViewControl.CoreWebView2?.Reload();
                        return;
                }
            }

            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.F)
            {
                e.Handled = true;
                FindBox.Focus();
                FindBox.SelectAll();
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.R)
            {                                   // Ctrl + Shift + R → hard refresh
                e.Handled = true;
                WebViewControl.CoreWebView2?.Reload();
                return;
            }

            // Alt + D → focus address bar (alternate shortcut)
            if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Alt)
            {
                e.Handled = true;
                AddressBar.Focus();
                AddressBar.SelectAll();
                return;
            }

            // -----------------------------------------------------------------
            // NAVIGATION
            // -----------------------------------------------------------------
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                switch (e.Key)
                {
                    case Key.F5:                // F5 → reload
                        e.Handled = true;
                        WebViewControl.CoreWebView2?.Reload();
                        return;
                }
            }

            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                switch (e.Key)
                {
                    case Key.Left:              // Alt + Left → back
                        e.Handled = true;
                        WebViewControl.CoreWebView2?.GoBack();
                        return;

                    case Key.Right:             // Alt + Right → forward
                        e.Handled = true;
                        WebViewControl.CoreWebView2?.GoForward();
                        return;
                }
            }

            // -----------------------------------------------------------------
            // DEV TOOLS
            // -----------------------------------------------------------------
            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.I)
            {
                e.Handled = true;
                DevToolsButton_Click(this, null);
                return;
            }

            if (Keyboard.Modifiers == ModifierKeys.None && e.Key == Key.F12)
            {
                e.Handled = true;
                DevToolsButton_Click(this, null);
                return;
            }

            if (Keyboard.Modifiers == (ModifierKeys.Control | ModifierKeys.Shift) && e.Key == Key.I)
            {
                e.Handled = true;
                DevToolsButton_Click(this, null);
                return;
            }

            base.OnKeyDown(e);
        }

        /* ---------------------------------------------------------------------
           Helper placeholders – wire these to your existing logic
        --------------------------------------------------------------------- */
        private void AddNewTab() { /* your tab-adding code */ }
        private void CloseCurrentTab() { /* your tab-closing code */ }
        private void SelectNextTab(int dir) { /* dir = +1 next, -1 prev */ }
        private void AutoCompleteUrl()
        {
            var text = AddressBar.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;

            if (!text.StartsWith("http"))
                text = "www." + text + ".com";

            AddressBar.Text = text.StartsWith("http") ? text : "https://" + text;
            NavigationHandler?.NavigateToAddressAsync(text);
        }
    }
}
