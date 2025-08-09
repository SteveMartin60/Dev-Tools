using System;
using System.Windows.Data;

namespace VideoThumbnailViewer
{
    public class WidthToColumnsConverter : IValueConverter
    {
        private const int ItemWidth = 520; // Width of each item including margins

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is double width)
            {
                // Calculate maximum number of complete items that fit
                int columns = (int)Math.Floor(width / ItemWidth);
                return Math.Max(1, columns); // Always show at least 1 column
            }
            return 1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
