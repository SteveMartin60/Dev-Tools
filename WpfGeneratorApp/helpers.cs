using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfCodeGenerator.Helpers
{
    public static class TextBoxHelper
    {
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.RegisterAttached(
                "PlaceholderText",
                typeof(string),
                typeof(TextBoxHelper),
                new PropertyMetadata(string.Empty, OnPlaceholderTextChanged));

        public static string GetPlaceholderText(DependencyObject obj) =>
            (string)obj.GetValue(PlaceholderTextProperty);

        public static void SetPlaceholderText(DependencyObject obj, string value) =>
            obj.SetValue(PlaceholderTextProperty, value);

        private static void OnPlaceholderTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBox textBox)
            {
                textBox.GotFocus += TextBox_GotFocus;
                textBox.LostFocus += TextBox_LostFocus;

                UpdatePlaceholder(textBox);
            }
        }

        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = string.Empty;
                textBox.Foreground = Brushes.Black; // Reset text color
            }
        }

        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && string.IsNullOrEmpty(textBox.Text))
            {
                UpdatePlaceholder(textBox);
            }
        }

        private static void UpdatePlaceholder(TextBox textBox)
        {
            string placeholder = GetPlaceholderText(textBox);
            if (!string.IsNullOrEmpty(placeholder) && string.IsNullOrEmpty(textBox.Text))
            {
                textBox.Text = placeholder;
                textBox.Foreground = Brushes.Gray; // Placeholder text color
            }
        }
    }
}
