using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

class IconExtractor
{
    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern int ExtractIconEx(
        string lpszFile,
        int nIconIndex,
        IntPtr[] phiconLarge,
        IntPtr[] phiconSmall,
        int nIcons
    );

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    public static void ExtractAndConvertIcon(int iconIndex, string dllPath, string outputDir)
    {
        // Ensure output directory exists
        Directory.CreateDirectory(outputDir);

        // Extract the icon from DLL
        IntPtr[] icons = new IntPtr[1];
        int count = ExtractIconEx(dllPath, iconIndex, icons, null, 1);

        if (count <= 0 || icons[0] == IntPtr.Zero)
        {
            Console.WriteLine($"Failed to extract icon index {iconIndex} from {dllPath}");
            return;
        }

        try
        {
            using (Icon icon = System.Drawing.Icon.FromHandle(icons[0]))
            {
                // Save as PNG
                string pngPath = Path.Combine(outputDir, $"icon_{iconIndex}.png");
                icon.ToBitmap().Save(pngPath, ImageFormat.Png);
                Console.WriteLine($"Saved PNG to: {pngPath}");

                // Convert to ICO
                string icoPath = Path.Combine(outputDir, $"icon_{iconIndex}.ico");
                PngToIconConverter.ConvertPngToIco(icon.ToBitmap(), icoPath);
                Console.WriteLine($"Saved ICO to: {icoPath}");
            }
        }
        finally
        {
            DestroyIcon(icons[0]);
        }
    }
}

class PngToIconConverter
{
    public static bool ConvertPngToIco(Bitmap sourceBitmap, string icoPath)
    {
        // Get the original dimensions from the PNG
        int width = sourceBitmap.Width;
        int height = sourceBitmap.Height;

        try
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var iconWriter = new BinaryWriter(memoryStream))
                {
                    // ICO header (6 bytes)
                    iconWriter.Write((short)0);  // Reserved
                    iconWriter.Write((short)1);  // Type (1 for ICO)
                    iconWriter.Write((short)1);  // Number of images (just this one)

                    // Image entry (16 bytes)
                    iconWriter.Write((byte)width);   // Width
                    iconWriter.Write((byte)height);  // Height
                    iconWriter.Write((byte)0);       // Color palette (0 for no palette)
                    iconWriter.Write((byte)0);       // Reserved
                    iconWriter.Write((short)1);      // Color planes
                    iconWriter.Write((short)32);     // Bits per pixel (32-bit ARGB)

                    // Convert the bitmap to PNG format for the ICO
                    byte[] imageData;
                    using (var bitmapStream = new MemoryStream())
                    {
                        sourceBitmap.Save(bitmapStream, ImageFormat.Png);
                        imageData = bitmapStream.ToArray();
                    }

                    iconWriter.Write(imageData.Length);  // Size of image data
                    iconWriter.Write(22);                // Offset to image data (6 + 16)
                    iconWriter.Write(imageData);         // The actual image data
                }

                File.WriteAllBytes(icoPath, memoryStream.ToArray());
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error converting to ICO: {ex.Message}");
            return false;
        }
    }
}

class Program
{
    static void Main()
    {
        int iconIndex = 124;
        string dllPath = Path.Combine(Environment.SystemDirectory, "imageres.dll");
        string outputDir = @"N:\OneDrive\Folders\Documents\ExtractedIcons\extracted_icons";

        using (var png = (Bitmap)Image.FromFile(@"N:\OneDrive\folders\Documents\ExtractedIcons\extracted_icons\icon_124.png"))
        {
            // Convert to ICO with same dimensions
            PngToIconConverter.ConvertPngToIco(png, @"N:\OneDrive\folders\Documents\ExtractedIcons\extracted_icons\icon_124.ico");

            Console.WriteLine($"Converted {png.Width}x{png.Height} PNG to ICO");
        }


        //IconExtractor.ExtractAndConvertIcon(iconIndex, dllPath, outputDir);
    }
}
