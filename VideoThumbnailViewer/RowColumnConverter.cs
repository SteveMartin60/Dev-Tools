// File: RowColumnConverter.cs
using System;
using System.Windows.Data;

namespace VideoThumbnailViewer
{
    public class RowColumnConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values[0] is int index && values[1] is int columns && columns > 0)
            {
                // Calculate column (cycle through available columns)
                int column = index % columns;
                // Calculate row (based on how many items fit in each column)
                int row = index / columns;
                return parameter.ToString() == "Row" ? row : column;
            }
            return 0; // Default to row 0 or column 0 if invalid
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
