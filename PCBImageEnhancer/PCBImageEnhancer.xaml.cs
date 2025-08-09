using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PCBImageEnhancer;

namespace PCBImageEnhancer
{
    public partial class MainWindow : Window
    {
        private Bitmap originalBitmap;
        private WriteableBitmap processedBitmap;

        public MainWindow()
        {
            InitializeComponent();
            LoadImage(@"D:\Dev\Image-Take\2024-04-26-14-43-CM-Scan\00.tif"); // Replace with your TIFF file path
            SetupEventHandlers();
        }

        private void LoadImage(string filePath)
        {
            try
            {
                originalBitmap = new Bitmap(filePath);
                var originalBitmapSource = ConvertBitmapToBitmapSource(originalBitmap);
                OriginalImage.Source = originalBitmapSource;

                // Initialize processed bitmap
                processedBitmap = ConvertBitmapToWriteableBitmap(originalBitmap);
                ProcessedImage.Source = processedBitmap;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading image: {ex.Message}");
            }
        }

        private void SetupEventHandlers()
        {
            BrightnessSlider.ValueChanged += (s, e) => UpdateImage();
            ContrastSlider.ValueChanged += (s, e) => UpdateImage();
            GammaSlider.ValueChanged += (s, e) => UpdateImage();
            HistogramEqualizationCheckbox.Checked += (s, e) => UpdateImage();
            HistogramEqualizationCheckbox.Unchecked += (s, e) => UpdateImage();
        }

        private void UpdateImage()
        {
            if (originalBitmap == null) return;

            Bitmap processed = ApplyEnhancements(originalBitmap);
            processedBitmap = ConvertBitmapToWriteableBitmap(processed);
            ProcessedImage.Source = processedBitmap;
        }

        private Bitmap ApplyEnhancements(Bitmap bitmap)
        {
            float brightness = (float)BrightnessSlider.Value / 100;
            float contrast = (float)ContrastSlider.Value / 100;
            float gamma = (float)GammaSlider.Value;
            bool histogramEqualization = HistogramEqualizationCheckbox.IsChecked == true;

            return ImageProcessor.Process(bitmap, brightness, contrast, gamma, histogramEqualization);
        }

        private BitmapSource ConvertBitmapToBitmapSource(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format16bppGrayScale);

            var bitmapSource = BitmapSource.Create(bitmap.Width, bitmap.Height, 96, 96, PixelFormats.Gray16, null, bitmapData.Scan0, bitmapData.Stride * bitmap.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);

            return bitmapSource;
        }

        private WriteableBitmap ConvertBitmapToWriteableBitmap(Bitmap bitmap)
        {
            var bitmapSource = ConvertBitmapToBitmapSource(bitmap);
            return new WriteableBitmap(bitmapSource);
        }
    }
}
