// WebViewSecuritySettings.cs
using Microsoft.Web.WebView2.Core;

namespace WebView2Browser
{
    public static class WebViewSecuritySettings
    {
        public static void ApplySecureSettings(CoreWebView2Settings settings)
        {
            settings.AreDefaultContextMenusEnabled = false;
            settings.AreDevToolsEnabled = false;
            settings.IsStatusBarEnabled = false;
            settings.AreBrowserAcceleratorKeysEnabled = true;
            settings.IsWebMessageEnabled = true;  // ✅ CHANGED: Must be true for find feature
            settings.AreHostObjectsAllowed = false;
            settings.IsZoomControlEnabled = false;
        }
    }
}
