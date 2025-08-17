using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using System.Collections.Generic;

namespace AvaloniaColorTableApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadColors();
        }

        private void LoadColors()
        {
            var colors = new List<ColorItem>
            {
                new ColorItem { Name = "AliceBlue", Hex = "#FFF0F8FF", RGB = "240, 248, 255", Brush = new SolidColorBrush(Color.Parse("#FFF0F8FF")) },
                new ColorItem { Name = "AntiqueWhite", Hex = "#FAEBD7", RGB = "250, 235, 215", Brush = new SolidColorBrush(Color.Parse("#FAEBD7")) },
                new ColorItem { Name = "Aqua", Hex = "#FF00FFFF", RGB = "0, 255, 255", Brush = new SolidColorBrush(Color.Parse("#FF00FFFF")) },
                // Add more colors as needed
            };

            //ColorDataGrid.ItemsSource = colors;
        }
    }

    public class ColorItem
    {
        public string Name { get; set; }
        public string Hex { get; set; }
        public string RGB { get; set; }
        public IBrush Brush { get; set; }
    }
}
