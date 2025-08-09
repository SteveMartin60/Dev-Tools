using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CodeConsolidator.Services
{
    public class ThemeService
    {
        private readonly Window _window;
        private readonly RichTextBox _codeDisplay;
        private readonly TextBlock _statusText;
        private readonly Panel _controlPanel;

        private bool _darkMode;

        public ThemeService(Window window, RichTextBox codeDisplay, TextBlock statusText, Panel controlPanel)
        {
            _window = window;
            _codeDisplay = codeDisplay;
            _statusText = statusText;
            _controlPanel = controlPanel;
        }

        public bool IsDarkMode()
        {
            return _darkMode;
        }

        public void ToggleDarkMode()
        {
            _darkMode = !_darkMode;

            _window.Background = _darkMode ? Brushes.DarkSlateGray : Brushes.White;
            _window.Foreground = _darkMode ? Brushes.White : Brushes.Black;

            _codeDisplay.Background = _darkMode ? Brushes.DimGray : Brushes.White;
            _codeDisplay.Foreground = _darkMode ? Brushes.White : Brushes.Black;

            _statusText.Foreground = _darkMode ? Brushes.LightGray : Brushes.Gray;

            foreach (var child in _controlPanel.Children)
            {
                if (child is Button button)
                {
                    button.Background = _darkMode ? Brushes.SlateGray : SystemColors.ControlBrush;
                    button.Foreground = _darkMode ? Brushes.White : SystemColors.ControlTextBrush;
                }
                else if (child is CheckBox checkBox)
                {
                    checkBox.Foreground = _darkMode ? Brushes.White : SystemColors.ControlTextBrush;
                }
            }
        }
    }
}
