using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;

namespace PCBxRayProcessor
{
    public partial class MainWindow : Window
    {
        private WriteableBitmap original16BitImage;
        private WriteableBitmap processed16BitImage;
        private bool isProcessing = false;
        private bool autoApply = true;

        public MainWindow()
        {
            InitializeComponent();
            ResetAdjustments();
        }

        private void LoadImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "TIFF Images|*.tif;*.tiff|All Files|*.*",
                Title = "Select a 16-bit Grayscale TIFF Image"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Load the original 16-bit image
                    original16BitImage = Load16BitTiff(openFileDialog.FileName);

                    // Display original with proper scaling
                    OriginalImage.Source = ConvertToDisplayableImage(original16BitImage);

                    // Create a copy for processing
                    processed16BitImage = new WriteableBitmap(original16BitImage);

                    // Display processed image (initially same as original)
                    ProcessedImage.Source = ConvertToDisplayableImage(processed16BitImage);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            OriginalImage.Source = ConvertToDisplayableImage(original16BitImage);
            PrintImageStats(original16BitImage);
        }

        private WriteableBitmap Load16BitTiff(string filePath)
        {
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                BitmapDecoder decoder = BitmapDecoder.Create(
                    stream,
                    BitmapCreateOptions.PreservePixelFormat,
                    BitmapCacheOption.OnLoad);

                BitmapFrame frame = decoder.Frames[0];

                if (frame.Format != PixelFormats.Gray16)
                {
                    FormatConvertedBitmap converted = new FormatConvertedBitmap(frame, PixelFormats.Gray16, null, 0);
                    return new WriteableBitmap(converted);
                }

                return new WriteableBitmap(frame);
            }
        }

        private BitmapSource ConvertTo8BitForDisplay(WriteableBitmap source16bit)
        {
            var displayBitmap = new WriteableBitmap(
                source16bit.PixelWidth, source16bit.PixelHeight,
                source16bit.DpiX, source16bit.DpiY,
                PixelFormats.Gray8, null);

            source16bit.Lock();
            displayBitmap.Lock();

            try
            {
                unsafe
                {
                    ushort* srcPixels = (ushort*)source16bit.BackBuffer;
                    byte* dstPixels = (byte*)displayBitmap.BackBuffer;
                    int pixelCount = source16bit.PixelWidth * source16bit.PixelHeight;

                    // Find min/max for auto scaling
                    ushort min = ushort.MaxValue, max = ushort.MinValue;
                    for (int i = 0; i < pixelCount; i++)
                    {
                        if (srcPixels[i] < min) min = srcPixels[i];
                        if (srcPixels[i] > max) max = srcPixels[i];
                    }

                    // Scale to 8-bit
                    double scale = 255.0 / (max - min);
                    for (int i = 0; i < pixelCount; i++)
                    {
                        dstPixels[i] = (byte)((srcPixels[i] - min) * scale);
                    }
                }
            }
            finally
            {
                source16bit.Unlock();
                displayBitmap.Unlock();
            }

            return displayBitmap;
        }

        private void Adjustment_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (autoApply && original16BitImage != null && !isProcessing)
            {
                ApplyProcessing();
            }
        }

        private void ApplyProcessing_Click(object sender, RoutedEventArgs e)
        {
            ApplyProcessing();
        }
        //.....................................................................

        //.....................................................................
        private void ApplyProcessing()
        {
            if (original16BitImage == null || isProcessing) return;

            isProcessing = true;

            try
            {
                // Make a fresh copy of the original
                processed16BitImage = new WriteableBitmap(original16BitImage);

                // Apply contrast stretch with histogram clipping
                double clipPercentage = HistogramClipSlider.Value;
                processed16BitImage = ApplyContrastAdjustment(processed16BitImage, clipPercentage);

                // Apply brightness/contrast/gamma
                processed16BitImage = ApplyLevelsAdjustment(
                    processed16BitImage,
                    BrightnessSlider.Value,
                    ContrastSlider.Value,
                    GammaSlider.Value);

                // Apply edge enhancement if needed
                if (EdgeEnhancementSlider.Value > 0)
                {
                    processed16BitImage = ApplyEdgeEnhancement(processed16BitImage, EdgeEnhancementSlider.Value);
                }

                // Apply sharpening if needed
                if (SharpenSlider.Value > 0)
                {
                    processed16BitImage = ApplySharpen(processed16BitImage, SharpenSlider.Value);
                }

                // Apply noise reduction if needed
                if (NoiseReductionSlider.Value > 0)
                {
                    processed16BitImage = ApplyNoiseReduction(processed16BitImage, NoiseReductionSlider.Value);
                }

                // Display the processed image
                ProcessedImage.Source = ConvertToDisplayableImage(processed16BitImage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                isProcessing = false;
            }
        }
        //.....................................................................

        //.....................................................................
        private WriteableBitmap ApplyContrastAdjustment(WriteableBitmap source, double clipPercentage)
        {
            var result = new WriteableBitmap(source.PixelWidth, source.PixelHeight, source.DpiX, source.DpiY, PixelFormats.Gray16, null);

            source.Lock();
            result.Lock();

            try
            {
                unsafe
                {
                    ushort* srcPixels = (ushort*)source.BackBuffer;
                    ushort* dstPixels = (ushort*)result.BackBuffer;
                    int pixelCount = source.PixelWidth * source.PixelHeight;

                    // Calculate histogram
                    int[] histogram = new int[65536];
                    for (int i = 0; i < pixelCount; i++)
                    {
                        histogram[srcPixels[i]]++;
                    }

                    // Find min/max with histogram clipping
                    int clipCount = (int)(pixelCount * clipPercentage / 100);
                    ushort min = 0, max = 65535;

                    // Find min (left clip)
                    int accumulated = 0;
                    for (int i = 0; i < 65536; i++)
                    {
                        accumulated += histogram[i];
                        if (accumulated > clipCount)
                        {
                            min = (ushort)i;
                            break;
                        }
                    }

                    // Find max (right clip)
                    accumulated = 0;
                    for (int i = 65535; i >= 0; i--)
                    {
                        accumulated += histogram[i];
                        if (accumulated > clipCount)
                        {
                            max = (ushort)i;
                            break;
                        }
                    }

                    // Apply contrast stretch with clipping
                    double scale = 65535.0 / (max - min);
                    for (int i = 0; i < pixelCount; i++)
                    {
                        ushort value = srcPixels[i];
                        if (value < min) value = min;
                        if (value > max) value = max;
                        dstPixels[i] = (ushort)((value - min) * scale);
                    }
                }
            }
            finally
            {
                source.Unlock();
                result.Unlock();
            }

            return result;
        }

        private WriteableBitmap ApplyLevelsAdjustment(WriteableBitmap source, double brightness, double contrast, double gamma)
        {
            var result = new WriteableBitmap(source.PixelWidth, source.PixelHeight,
                                           source.DpiX, source.DpiY, PixelFormats.Gray16, null);

            source.Lock();
            result.Lock();

            try
            {
                unsafe
                {
                    ushort* srcPixels = (ushort*)source.BackBuffer;
                    ushort* dstPixels = (ushort*)result.BackBuffer;
                    int pixelCount = source.PixelWidth * source.PixelHeight;

                    for (int i = 0; i < pixelCount; i++)
                    {
                        double value = srcPixels[i] / 65535.0;

                        // Apply brightness
                        value += brightness;

                        // Apply contrast
                        value = ((value - 0.5) * contrast) + 0.5;

                        // Apply gamma
                        value = Math.Pow(value, 1.0 / gamma);

                        // Clamp and convert back to 16-bit
                        value = Math.Max(0, Math.Min(1, value));
                        dstPixels[i] = (ushort)(value * 65535);
                    }
                }
            }
            finally
            {
                source.Unlock();
                result.Unlock();
            }

            return result;
        }

        private WriteableBitmap ApplyEdgeEnhancement(WriteableBitmap source, double strength)
        {
            // Simple Sobel edge enhancement implementation
            var result = new WriteableBitmap(source.PixelWidth, source.PixelHeight,
                                           source.DpiX, source.DpiY, PixelFormats.Gray16, null);

            source.Lock();
            result.Lock();

            try
            {
                unsafe
                {
                    ushort* srcPixels = (ushort*)source.BackBuffer;
                    ushort* dstPixels = (ushort*)result.BackBuffer;
                    int width = source.PixelWidth;
                    int height = source.PixelHeight;

                    // Skip border pixels for simplicity
                    for (int y = 1; y < height - 1; y++)
                    {
                        for (int x = 1; x < width - 1; x++)
                        {
                            int index = y * width + x;

                            // Sobel kernels
                            int gx =
                                -1 * srcPixels[(y - 1) * width + (x - 1)] +
                                -2 * srcPixels[y * width + (x - 1)] +
                                -1 * srcPixels[(y + 1) * width + (x - 1)] +
                                1 * srcPixels[(y - 1) * width + (x + 1)] +
                                2 * srcPixels[y * width + (x + 1)] +
                                1 * srcPixels[(y + 1) * width + (x + 1)];

                            int gy =
                                -1 * srcPixels[(y - 1) * width + (x - 1)] +
                                -2 * srcPixels[(y - 1) * width + x] +
                                -1 * srcPixels[(y - 1) * width + (x + 1)] +
                                1 * srcPixels[(y + 1) * width + (x - 1)] +
                                2 * srcPixels[(y + 1) * width + x] +
                                1 * srcPixels[(y + 1) * width + (x + 1)];

                            // Calculate gradient magnitude
                            double edgeValue = Math.Sqrt(gx * gx + gy * gy) / (4 * 65535);

                            // Blend with original based on strength
                            double originalValue = srcPixels[index] / 65535.0;
                            double enhancedValue = originalValue + edgeValue * strength;
                            enhancedValue = Math.Max(0, Math.Min(1, enhancedValue));

                            dstPixels[index] = (ushort)(enhancedValue * 65535);
                        }
                    }

                    // Copy border pixels unchanged
                    for (int x = 0; x < width; x++)
                    {
                        dstPixels[x] = srcPixels[x]; // top
                        dstPixels[(height - 1) * width + x] = srcPixels[(height - 1) * width + x]; // bottom
                    }
                    for (int y = 0; y < height; y++)
                    {
                        dstPixels[y * width] = srcPixels[y * width]; // left
                        dstPixels[y * width + width - 1] = srcPixels[y * width + width - 1]; // right
                    }
                }
            }
            finally
            {
                source.Unlock();
                result.Unlock();
            }

            return result;
        }

        private WriteableBitmap ApplySharpen(WriteableBitmap source, double amount)
        {
            // Simple unsharp masking implementation
            var blurred = ApplyGaussianBlur(source, 1);
            var result = new WriteableBitmap(source.PixelWidth, source.PixelHeight,
                                           source.DpiX, source.DpiY, PixelFormats.Gray16, null);

            source.Lock();
            blurred.Lock();
            result.Lock();

            try
            {
                unsafe
                {
                    ushort* srcPixels = (ushort*)source.BackBuffer;
                    ushort* blurPixels = (ushort*)blurred.BackBuffer;
                    ushort* dstPixels = (ushort*)result.BackBuffer;
                    int pixelCount = source.PixelWidth * source.PixelHeight;

                    for (int i = 0; i < pixelCount; i++)
                    {
                        double original = srcPixels[i] / 65535.0;
                        double blur = blurPixels[i] / 65535.0;
                        double sharpened = original + (original - blur) * amount;
                        sharpened = Math.Max(0, Math.Min(1, sharpened));
                        dstPixels[i] = (ushort)(sharpened * 65535);
                    }
                }
            }
            finally
            {
                source.Unlock();
                blurred.Unlock();
                result.Unlock();
            }

            return result;
        }

        private WriteableBitmap ApplyGaussianBlur(WriteableBitmap source, double radius)
        {
            // Simple Gaussian blur implementation
            var result = new WriteableBitmap(source.PixelWidth, source.PixelHeight,
                                           source.DpiX, source.DpiY, PixelFormats.Gray16, null);

            // Horizontal pass
            var temp = new WriteableBitmap(source.PixelWidth, source.PixelHeight,
                                         source.DpiX, source.DpiY, PixelFormats.Gray16, null);

            int kernelSize = (int)(radius * 2) + 1;
            double[] kernel = CreateGaussianKernel(kernelSize, radius);

            source.Lock();
            temp.Lock();

            // Horizontal blur
            try
            {
                unsafe
                {
                    ushort* srcPixels = (ushort*)source.BackBuffer;
                    ushort* tempPixels = (ushort*)temp.BackBuffer;
                    int width = source.PixelWidth;
                    int height = source.PixelHeight;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            double sum = 0;
                            double weightSum = 0;

                            for (int i = 0; i < kernelSize; i++)
                            {
                                int px = x + i - kernelSize / 2;
                                if (px >= 0 && px < width)
                                {
                                    double weight = kernel[i];
                                    sum += srcPixels[y * width + px] * weight;
                                    weightSum += weight;
                                }
                            }

                            tempPixels[y * width + x] = (ushort)(sum / weightSum);
                        }
                    }
                }
            }
            finally
            {
                source.Unlock();
                temp.Unlock();
            }

            // Vertical blur
            temp.Lock();
            result.Lock();

            try
            {
                unsafe
                {
                    ushort* tempPixels = (ushort*)temp.BackBuffer;
                    ushort* dstPixels = (ushort*)result.BackBuffer;
                    int width = source.PixelWidth;
                    int height = source.PixelHeight;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            double sum = 0;
                            double weightSum = 0;

                            for (int i = 0; i < kernelSize; i++)
                            {
                                int py = y + i - kernelSize / 2;
                                if (py >= 0 && py < height)
                                {
                                    double weight = kernel[i];
                                    sum += tempPixels[py * width + x] * weight;
                                    weightSum += weight;
                                }
                            }

                            dstPixels[y * width + x] = (ushort)(sum / weightSum);
                        }
                    }
                }
            }
            finally
            {
                temp.Unlock();
                result.Unlock();
            }

            return result;
        }

        private double[] CreateGaussianKernel(int size, double sigma)
        {
            double[] kernel = new double[size];
            double sum = 0;
            int half = size / 2;

            for (int i = 0; i < size; i++)
            {
                double x = i - half;
                kernel[i] = Math.Exp(-(x * x) / (2 * sigma * sigma));
                sum += kernel[i];
            }

            // Normalize
            for (int i = 0; i < size; i++)
            {
                kernel[i] /= sum;
            }

            return kernel;
        }

        private WriteableBitmap ApplyNoiseReduction(WriteableBitmap source, double amount)
        {
            // Simple bilateral filter implementation
            var result = new WriteableBitmap(source.PixelWidth, source.PixelHeight,
                                           source.DpiX, source.DpiY, PixelFormats.Gray16, null);

            int radius = 2;
            double spatialSigma = 1.0;
            double intensitySigma = amount * 65535;

            source.Lock();
            result.Lock();

            try
            {
                unsafe
                {
                    ushort* srcPixels = (ushort*)source.BackBuffer;
                    ushort* dstPixels = (ushort*)result.BackBuffer;
                    int width = source.PixelWidth;
                    int height = source.PixelHeight;

                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            double sum = 0;
                            double weightSum = 0;
                            int centerValue = srcPixels[y * width + x];

                            for (int dy = -radius; dy <= radius; dy++)
                            {
                                for (int dx = -radius; dx <= radius; dx++)
                                {
                                    int nx = x + dx;
                                    int ny = y + dy;

                                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                                    {
                                        int neighborValue = srcPixels[ny * width + nx];

                                        // Spatial weight
                                        double spatialWeight = Math.Exp(-(dx * dx + dy * dy) / (2 * spatialSigma * spatialSigma));

                                        // Intensity weight
                                        double intensityWeight = Math.Exp(-Math.Pow(neighborValue - centerValue, 2) /
                                                                         (2 * intensitySigma * intensitySigma));

                                        double totalWeight = spatialWeight * intensityWeight;
                                        sum += neighborValue * totalWeight;
                                        weightSum += totalWeight;
                                    }
                                }
                            }

                            dstPixels[y * width + x] = (ushort)(sum / weightSum);
                        }
                    }
                }
            }
            finally
            {
                source.Unlock();
                result.Unlock();
            }

            return result;
        }

        private void ResetAdjustments_Click(object sender, RoutedEventArgs e)
        {
            ResetAdjustments();
            if (original16BitImage != null)
            {
                ApplyProcessing();
            }
        }

        private void ResetAdjustments()
        {
            BrightnessSlider.Value = 0;
            ContrastSlider.Value = 1;
            GammaSlider.Value = 1;
            HistogramClipSlider.Value = 0.5;
            EdgeEnhancementSlider.Value = 0;
            SharpenSlider.Value = 0;
            NoiseReductionSlider.Value = 0;
        }

        public bool AutoApply
        {
            get { return autoApply; }
            set { autoApply = value; }
        }
        private BitmapSource ConvertToDisplayableImage(WriteableBitmap source16bit)
        {
            // First, create a copy of the original to work with
            var workingCopy = new WriteableBitmap(source16bit);

            // Calculate min/max values for auto-scaling
            workingCopy.Lock();
            ushort min = ushort.MaxValue;
            ushort max = ushort.MinValue;

            unsafe
            {
                ushort* pixels = (ushort*)workingCopy.BackBuffer;
                int pixelCount = workingCopy.PixelWidth * workingCopy.PixelHeight;

                for (int i = 0; i < pixelCount; i++)
                {
                    if (pixels[i] < min) min = pixels[i];
                    if (pixels[i] > max) max = pixels[i];
                }
            }

            // Special case: if image is completely uniform
            if (min == max)
            {
                workingCopy.Unlock();
                return new FormatConvertedBitmap(workingCopy, PixelFormats.Gray8, null, 0);
            }

            // Create 8-bit version with proper scaling
            var displayBitmap = new WriteableBitmap(
                workingCopy.PixelWidth, workingCopy.PixelHeight,
                workingCopy.DpiX, workingCopy.DpiY,
                PixelFormats.Gray8, null);

            displayBitmap.Lock();

            unsafe
            {
                ushort* srcPixels = (ushort*)workingCopy.BackBuffer;
                byte* dstPixels = (byte*)displayBitmap.BackBuffer;
                int pixelCount = workingCopy.PixelWidth * workingCopy.PixelHeight;

                double scale = 255.0 / (max - min);
                for (int i = 0; i < pixelCount; i++)
                {
                    // Apply contrast stretch
                    ushort value = srcPixels[i];
                    double stretched = (value - min) * scale;
                    dstPixels[i] = (byte)Math.Min(255, Math.Max(0, stretched));
                }
            }

            workingCopy.Unlock();
            displayBitmap.Unlock();

            return displayBitmap;
        }
        private void PrintImageStats(WriteableBitmap image)
        {
            image.Lock();
            unsafe
            {
                ushort* pixels = (ushort*)image.BackBuffer;
                int pixelCount = image.PixelWidth * image.PixelHeight;

                ushort min = ushort.MaxValue;
                ushort max = ushort.MinValue;
                double sum = 0;

                for (int i = 0; i < pixelCount; i++)
                {
                    if (pixels[i] < min) min = pixels[i];
                    if (pixels[i] > max) max = pixels[i];
                    sum += pixels[i];
                }

                double avg = sum / pixelCount;

                Console.WriteLine($"Image stats - Min: {min}, Max: {max}, Avg: {avg}");
                Console.WriteLine($"Dynamic range used: {(max - min) / 65535.0 * 100:0.00}%");
            }
            image.Unlock();
        }
    }

}
