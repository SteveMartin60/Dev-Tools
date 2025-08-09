using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

public static class IconsToComposite
{
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int ExtractIconEx(
        string lpszFile,
        int nIconIndex,
        out IntPtr phiconLarge,
        out IntPtr phiconSmall,
        uint nIcons);

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public static void ExtractIconsToCompositeImage(string dllPath, string outputDirectory)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dllPath))
        {
            throw new ArgumentException("DLL path must be specified");
        }

        if (!File.Exists(dllPath))
        {
            throw new FileNotFoundException($"File not found: {dllPath}");
        }

        // Count icons in the DLL
        int totalIcons = ExtractIconEx(dllPath, -1, out IntPtr _, out IntPtr _, 0);
        if (totalIcons <= 0)
        {
            throw new InvalidOperationException("No icons found in the DLL");
        }

        string filenameWithoutExtension = Path.GetFileNameWithoutExtension(dllPath);
        Directory.CreateDirectory(outputDirectory);

        // Lists to store extracted icons
        List<Bitmap> iconBitmaps = new List<Bitmap>();
        List<Icon> icons = new List<Icon>();

        // Extract all icons with proper color depth
        for (int iconIndex = 0; iconIndex < totalIcons; iconIndex++)
        {
            ExtractIconEx(dllPath, iconIndex, out IntPtr largeIconHandle, out IntPtr smallIconHandle, 1);

            try
            {
                if (largeIconHandle != IntPtr.Zero)
                {
                    // Create icon preserving original color depth
                    using (Icon originalIcon = Icon.FromHandle(largeIconHandle))
                    {
                        // Store the original icon with full color depth
                        icons.Add(new Icon(originalIcon, originalIcon.Size));

                        // Create high-quality bitmap with alpha channel
                        Bitmap bitmap = new Bitmap(
                            originalIcon.Width * 2,
                            originalIcon.Height * 2,
                            PixelFormat.Format32bppArgb);

                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = SmoothingMode.HighQuality;
                            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            g.CompositingQuality = CompositingQuality.HighQuality;
                            g.Clear(Color.Transparent);
                            g.DrawIcon(originalIcon, new Rectangle(0, 0, bitmap.Width, bitmap.Height));
                        }
                        iconBitmaps.Add(bitmap);
                    }
                }
            }
            finally
            {
                if (largeIconHandle != IntPtr.Zero) DestroyIcon(largeIconHandle);
                if (smallIconHandle != IntPtr.Zero) DestroyIcon(smallIconHandle);
            }
        }

        // Save individual icons with full color depth
        string individualIconsDir = Path.Combine(outputDirectory, "individual_icons");
        Directory.CreateDirectory(individualIconsDir);

        for (int i = 0; i < icons.Count; i++)
        {
            // Save as PNG (already preserves color depth)
            string pngPath = Path.Combine(individualIconsDir, $"icon_{i}.png");

            using (Bitmap bmp = new Bitmap(icons[i].ToBitmap()))
            {
                bmp.Save(pngPath, ImageFormat.Png);
            }
        }

        // Cleanup
        foreach (var bmp in iconBitmaps) bmp.Dispose();
        foreach (var icon in icons) icon.Dispose();
    }
}