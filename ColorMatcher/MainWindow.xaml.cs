using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace ColorMatcher
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Dictionary<string, Color> _selectedColors;
        private readonly ColorManager _colorManager;
        private string _selectedColorSet = "All";

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _colorManager = new ColorManager();
            _colorManager.LoadAllColorSets();
            UpdateColorSetInUse();
        }

        public List<string> AvailableColorSets { get; } = new List<string>
        {
            "All",
            "CSS",
            "Material",
            "X11",
            "Pantone"
        };

        public string SelectedColorSet
        {
            get => _selectedColorSet;
            set
            {
                if (_selectedColorSet != value)
                {
                    _selectedColorSet = value;
                    OnPropertyChanged(nameof(SelectedColorSet));
                    UpdateColorSetInUse();
                }
            }
        }

        private SolidColorBrush _customColor = new SolidColorBrush(Colors.White);
        public SolidColorBrush CustomColor
        {
            get => _customColor;
            set
            {
                if (_customColor != value)
                {
                    _customColor = value;
                    OnPropertyChanged(nameof(CustomColor));
                }
            }
        }

        private string _customColorHex = "#FFFFFF";
        public string CustomColorHex
        {
            get => _customColorHex;
            set
            {
                if (_customColorHex != value)
                {
                    _customColorHex = value;
                    OnPropertyChanged(nameof(CustomColorHex));
                }
            }
        }

        private SolidColorBrush _closestStandardColor = new SolidColorBrush(Colors.White);
        public SolidColorBrush ClosestStandardColor
        {
            get => _closestStandardColor;
            set
            {
                if (_closestStandardColor != value)
                {
                    _closestStandardColor = value;
                    OnPropertyChanged(nameof(ClosestStandardColor));
                }
            }
        }

        private string _closestStandardColorName = "White";
        public string ClosestStandardColorName
        {
            get => _closestStandardColorName;
            set
            {
                if (_closestStandardColorName != value)
                {
                    _closestStandardColorName = value;
                    OnPropertyChanged(nameof(ClosestStandardColorName));
                }
            }
        }

        private string _closestStandardColorHex = "#FFFFFF";
        public string ClosestStandardColorHex
        {
            get => _closestStandardColorHex;
            set
            {
                if (_closestStandardColorHex != value)
                {
                    _closestStandardColorHex = value;
                    OnPropertyChanged(nameof(ClosestStandardColorHex));
                }
            }
        }

        private async void DownloadCssColors_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Downloading CSS colors...";
                StatusText.Foreground = new SolidColorBrush(Colors.Black);

                await _colorManager.DownloadCssColorsAsync();
                UpdateColorSetInUse();

                if (_colorManager.ColorsLoaded)
                {
                    StatusText.Text = "CSS Colors Downloaded Successfully!";
                    StatusText.Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    StatusText.Text = "Download completed but no colors were loaded";
                    StatusText.Foreground = new SolidColorBrush(Colors.Orange);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading CSS colors: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Failed to Download CSS Colors";
                StatusText.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private async void DownloadAllColorSets_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                StatusText.Text = "Downloading all color sets...";
                StatusText.Foreground = new SolidColorBrush(Colors.Black);

                await _colorManager.DownloadAllColorSetsAsync();
                UpdateColorSetInUse();

                if (_colorManager.ColorsLoaded)
                {
                    StatusText.Text = "All color sets downloaded successfully!";
                    StatusText.Foreground = new SolidColorBrush(Colors.Green);
                }
                else
                {
                    StatusText.Text = "Download completed but no colors were loaded";
                    StatusText.Foreground = new SolidColorBrush(Colors.Orange);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error downloading color sets: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                StatusText.Text = "Failed to download color sets";
                StatusText.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void UpdateColorSetInUse()
        {
            var allColors = _colorManager.GetAllColors();
            if (allColors.Count == 0)
            {
                _selectedColors = new Dictionary<string, Color>();
                return;
            }

            _selectedColors = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);

            switch (SelectedColorSet)
            {
                case "CSS":
                    _selectedColors = allColors
                        .Where(kv => _colorManager.IsCssColor(kv.Key))
                        .ToDictionary(kv => kv.Key, kv => kv.Value);
                    break;
                case "Material":
                    _selectedColors = allColors
                        .Where(kv => _colorManager.IsMaterialColor(kv.Key))
                        .ToDictionary(kv => kv.Key, kv => kv.Value);
                    break;
                case "X11":
                    _selectedColors = allColors
                        .Where(kv => _colorManager.IsX11Color(kv.Key))
                        .ToDictionary(kv => kv.Key, kv => kv.Value);
                    break;
                case "Pantone":
                    _selectedColors = allColors
                        .Where(kv => _colorManager.IsPantoneColor(kv.Key))
                        .ToDictionary(kv => kv.Key, kv => kv.Value);
                    break;
                default: // "All"
                    _selectedColors = new Dictionary<string, Color>(allColors);
                    break;
            }
        }

        private void FindClosestColor_Click(object sender, RoutedEventArgs e)
        {
            if (!_colorManager.ColorsLoaded)
            {
                MessageBox.Show("No colors available. Please download the colors first.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string hexInput = HexInput.Text.Trim();

            // Ensure input starts with #
            if (!hexInput.StartsWith("#"))
            {
                hexInput = "#" + hexInput;
            }

            // Validate format
            if (!Regex.IsMatch(hexInput, "^#([A-Fa-f0-9]{6})$"))
            {
                MessageBox.Show("Invalid Hex Color Format. Please use RRGGBB or #RRGGBB.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Update UI to show the corrected format if needed
            if (HexInput.Text != hexInput)
            {
                HexInput.Text = hexInput;
            }

            int red = Convert.ToInt32(hexInput.Substring(1, 2), 16);
            int green = Convert.ToInt32(hexInput.Substring(3, 2), 16);
            int blue = Convert.ToInt32(hexInput.Substring(5, 2), 16);

            // Update the custom color
            CustomColor = new SolidColorBrush(Color.FromRgb((byte)red, (byte)green, (byte)blue));
            CustomColorHex = hexInput;

            // Find the closest standard color
            var closestColor = GetClosestStandardColor(red, green, blue);
            ClosestStandardColor = new SolidColorBrush(closestColor.Color);
            ClosestStandardColorName = closestColor.Name;
            ClosestStandardColorHex = $"#{closestColor.Color.R:X2}{closestColor.Color.G:X2}{closestColor.Color.B:X2}";
        }

        private (string Name, Color Color) GetClosestStandardColor(int r, int g, int b)
        {
            const double redWeight = 0.299;
            const double greenWeight = 0.587;
            const double blueWeight = 0.114;

            var closest = _selectedColors.MinBy(entry =>
            {
                double dr = entry.Value.R - r;
                double dg = entry.Value.G - g;
                double db = entry.Value.B - b;
                return Math.Pow(dr * redWeight, 2) +
                       Math.Pow(dg * greenWeight, 2) +
                       Math.Pow(db * blueWeight, 2);
            });

            return (closest.Key, closest.Value);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}