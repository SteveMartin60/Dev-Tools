using System.Runtime.InteropServices;
using System.Windows;

namespace WebView2Browser
{
    public partial class MainWindow : Window
    {
        internal static class KeyboardHelper
        {
            [DllImport("user32.dll")]
            private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

            private const uint WM_KEYDOWN  = 0x0100;
            private const uint WM_KEYUP    = 0x0101;
            private const int  VK_CONTROL  = 0x11;
            private const int  VK_F        = 0x46;

            public static void SendCtrlF(IntPtr hWnd)
            {
                PostMessage(hWnd, WM_KEYDOWN, VK_CONTROL, IntPtr.Zero);
                PostMessage(hWnd, WM_KEYDOWN, VK_F,       IntPtr.Zero);
                PostMessage(hWnd, WM_KEYUP,   VK_F,       IntPtr.Zero);
                PostMessage(hWnd, WM_KEYUP,   VK_CONTROL, IntPtr.Zero);
            }
        }
    }
}
