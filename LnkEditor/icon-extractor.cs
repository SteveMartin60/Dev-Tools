using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace LnkEditor
{
    public partial class link_editor : Window
    {
        //.....................................................................
        #region ----------  Native icon extraction  ----------
        //.....................................................................
        [DllImport("shell32.dll", CharSet = CharSet.Auto)] private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);
        //.....................................................................
        [DllImport("user32.dll", SetLastError = true)] private static extern bool DestroyIcon(IntPtr hIcon);
        //.....................................................................
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        //.....................................................................
        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_SMALLICON = 0x1;
        private const uint SHGFI_LARGEICON = 0x0;
        //.....................................................................
        #endregion
        //.....................................................................
        #region ----------  Icon extraction  ----------
        //.....................................................................
        private List<BitmapSource> ExtractIcons(string filePath)
        {
            var icons = new List<BitmapSource>();
            try
            {
                for (int i = 0; i < 20; i++)
                {
                    SHFILEINFO shinfo = new SHFILEINFO();
                    IntPtr hImgSmall = SHGetFileInfo(filePath, 0, ref shinfo,
                                                     (uint)Marshal.SizeOf(shinfo),
                                                     SHGFI_ICON | SHGFI_SMALLICON | (uint)(i << 16));

                    if (shinfo.hIcon != IntPtr.Zero)
                    {
                        using (System.Drawing.Icon icon = System.Drawing.Icon.FromHandle(shinfo.hIcon))
                        {
                            BitmapSource bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                                icon.Handle,
                                new Int32Rect(0, 0, icon.Width, icon.Height),
                                BitmapSizeOptions.FromEmptyOptions());
                            icons.Add(bitmapSource);
                        }
                        DestroyIcon(shinfo.hIcon);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error extracting icons: {ex.Message}", "Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return icons;
        }
        //.....................................................................
        #endregion
        //.....................................................................
    }
}
