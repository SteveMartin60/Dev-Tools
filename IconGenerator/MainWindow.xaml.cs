using Microsoft.Win32;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;

namespace IconGenerator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        // Browse for the input PNG file
        private void BrowseInputFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "PNG Files (*.png)|*.png|All Files (*.*)|*.*",
                Title = "Select Input PNG File"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                InputFilePath.Text = openFileDialog.FileName;
            }
        }

        // Browse for the output ICO file path
        private void BrowseOutputFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Icon Files (*.ico)|*.ico|All Files (*.*)|*.*",
                Title = "Save Icon File As"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                OutputFilePath.Text = saveFileDialog.FileName;
            }
        }

        // Generate the icon file
        private void GenerateIcon_Click(object sender, RoutedEventArgs e)
        {
            string inputPath = InputFilePath.Text;
            string outputPath = OutputFilePath.Text;

            if (string.IsNullOrEmpty(inputPath) || string.IsNullOrEmpty(outputPath))
            {
                StatusMessage.Text = "Please select both input and output files.";
                StatusMessage.Foreground = System.Windows.Media.Brushes.Red;
                return;
            }

            try
            {
                // Define the required icon sizes
                int[] iconSizes = { 512, 256, 128, 64, 32 };

                // Load the original image
                using (Bitmap originalImage = new Bitmap(inputPath))
                using (var memoryStream = new MemoryStream())
                {
                    foreach (int size in iconSizes)
                    {
                        using (Bitmap resizedImage = ResizeImage(originalImage, size, size))
                        {
                            // Save the resized image to the memory stream as PNG
                            resizedImage.Save(memoryStream, ImageFormat.Png);
                        }
                    }

                    // Write the final .ico file
                    using (FileStream fileStream = new FileStream(outputPath, FileMode.Create))
                    {
                        memoryStream.WriteTo(fileStream);
                    }
                }

                StatusMessage.Text = "Icon created successfully!";
                StatusMessage.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch (Exception ex)
            {
                StatusMessage.Text = $"Error: {ex.Message}";
                StatusMessage.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        // Helper method to resize an image
        private static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(System.Drawing.Drawing2D.WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }
    }
}
