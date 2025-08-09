using System;
using System.Globalization;
using System.Windows.Data;

namespace FastFileCopier  // Make sure this matches your actual namespace
{
    public class SizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "0 B";

            long bytes = System.Convert.ToInt64(value);

            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (bytes >= 1024 && order < sizes.Length - 1)
            {
                order++;
                bytes = bytes / 1024;
            }

            return $"{bytes:0.##} {sizes[order]}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
