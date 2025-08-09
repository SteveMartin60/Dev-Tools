using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace VideoThumbnailViewer
{
    public class ThumbnailPathConverter : IValueConverter
    {
        private static readonly BitmapImage _placeholder;

        static ThumbnailPathConverter()
        {
            _placeholder = CreatePlaceholderImage();
            _placeholder.Freeze(); // Make it thread-safe
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            try
            {
                string? path = value as string;

                // If no path or empty, return placeholder
                if (string.IsNullOrWhiteSpace(path))
                    return _placeholder;

                // If path is already a pack URI, use it directly
                if (path.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                {
                    var result = CreateBitmapImage(new Uri(path, UriKind.Absolute));

                    if (result != null)
                        return result;
                    else
                        return _placeholder;
                }

                // Check if file exists and is accessible
                if (!File.Exists(path) || IsFileLocked(path))
                    return _placeholder;

                // Try to create the bitmap
                var uri = new Uri(path, UriKind.Absolute);
                return CreateBitmapImage(uri) ?? _placeholder;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Thumbnail conversion failed: {ex.Message}");
                return _placeholder;
            }
        }

        private static BitmapImage? CreateBitmapImage(Uri uri)
        {
            BitmapImage? bitmap = null;
            try
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bitmap.UriSource = uri;
                bitmap.DecodePixelWidth = 500;
                bitmap.EndInit();

                // Verify the image was created successfully
                if (bitmap.Width <= 0 || bitmap.Height <= 0)
                    throw new InvalidOperationException("Invalid image dimensions");

                return bitmap;
            }
            catch
            {
                bitmap?.StreamSource?.Dispose();
                return null;
            }
            finally
            {
                if (bitmap != null && !bitmap.IsFrozen && bitmap.CanFreeze)
                {
                    bitmap.Freeze();
                }
            }
        }

        private static BitmapImage CreatePlaceholderImage()
        {
            try
            {
                var placeholder = new BitmapImage();
                placeholder.BeginInit();
                placeholder.UriSource = new Uri("pack://application:,,,/Resources/placeholder.png", UriKind.Absolute);
                placeholder.CacheOption = BitmapCacheOption.OnLoad;
                placeholder.EndInit();
                return placeholder;
            }
            catch
            {
                // Fallback to a blank image if even the placeholder fails
                return new BitmapImage();
            }
        }

        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                
                return false;
            }
            catch (IOException)
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
