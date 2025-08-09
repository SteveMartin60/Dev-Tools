using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace PCBImageEnhancer
{
    public static class ImageProcessor
    {
        public static Bitmap Process(Bitmap input, float brightness, float contrast, float gamma, bool histogramEqualization)
        {
            // Apply brightness, contrast, and gamma adjustments
            Bitmap adjustedBitmap = AdjustBrightnessContrastGamma(input, brightness, contrast, gamma);

            // Apply histogram equalization if enabled
            if (histogramEqualization)
            {
                adjustedBitmap = HistogramEqualization(adjustedBitmap);
            }

            return adjustedBitmap;
        }

        private static Bitmap AdjustBrightnessContrastGamma(Bitmap input, float brightness, float contrast, float gamma)
        {
            // Create a blank output bitmap
            Bitmap output = new Bitmap(input.Width, input.Height, PixelFormat.Format16bppGrayScale);

            // Lock the input and output bitmaps for processing
            var inputBitmapData = input.LockBits(new Rectangle(0, 0, input.Width, input.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format16bppGrayScale);
            var outputBitmapData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height),
                ImageLockMode.WriteOnly, PixelFormat.Format16bppGrayScale);

            unsafe
            {
                // Get pointers to the first pixel of the input and output bitmaps
                ushort* inputPtr = (ushort*)inputBitmapData.Scan0.ToPointer();
                ushort* outputPtr = (ushort*)outputBitmapData.Scan0.ToPointer();

                int stride = inputBitmapData.Stride / 2; // Divide by 2 because each pixel is 16 bits (2 bytes)

                // Loop through all pixels
                for (int y = 0; y < input.Height; y++)
                {
                    for (int x = 0; x < input.Width; x++)
                    {
                        // Read the pixel value
                        ushort pixelValue = inputPtr[y * stride + x];

                        // Apply brightness adjustment (-100 to 100 range)
                        pixelValue = (ushort)Math.Min(65535, Math.Max(0, pixelValue + (brightness * 256)));

                        // Apply contrast adjustment (0 to 200 range)
                        pixelValue = (ushort)((pixelValue - 32768) * contrast + 32768);
                        pixelValue = (ushort)Math.Min((ushort)65535, Math.Max((ushort)0, pixelValue + (brightness * 256)));

                        // Apply gamma correction (0.1 to 3.0 range)
                        double normalizedValue = pixelValue / 65535.0;
                        normalizedValue = Math.Pow(normalizedValue, 1.0 / gamma);
                        pixelValue = (ushort)(normalizedValue * 65535);

                        // Write the adjusted pixel value to the output bitmap
                        outputPtr[y * stride + x] = pixelValue;
                    }
                }
            }

            // Unlock the bitmaps
            input.UnlockBits(inputBitmapData);
            output.UnlockBits(outputBitmapData);

            return output;
        }

        private static Bitmap HistogramEqualization(Bitmap input)
        {
            // Placeholder for histogram equalization logic
            // Implement this method based on your requirements
            return input; // Replace with actual histogram equalization implementation
        }
    }
}
