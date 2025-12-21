using System.Windows;
using System.Windows.Media;
using DrawingColor = System.Drawing.Color;

namespace ScreenColorPicker
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UpdateSelectedColor(Colors.Transparent);
        }

        private void PickColor_Click(object sender, RoutedEventArgs e)
        {
            var overlay = new PickerOverlay();
            overlay.ColorPicked += Overlay_ColorPicked;
            overlay.LiveColorChanged += Overlay_LiveColorChanged;
            overlay.Owner = this;

            overlay.Show();
        }

        private void Overlay_LiveColorChanged(DrawingColor drawingColor)
        {
            var mediaColor = Color.FromArgb(
                drawingColor.A,
                drawingColor.R,
                drawingColor.G,
                drawingColor.B);

            UpdateSelectedColor(mediaColor);
        }

        private void Overlay_ColorPicked(DrawingColor drawingColor)
        {
            var mediaColor = Color.FromArgb(
                drawingColor.A,
                drawingColor.R,
                drawingColor.G,
                drawingColor.B);

            UpdateSelectedColor(mediaColor);
        }

        private void UpdateSelectedColor(Color color)
        {
            ColorPreview.Background = new SolidColorBrush(color);
            RgbText.Text = $"R: {color.R}, G: {color.G}, B: {color.B}";
            HexText.Text = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }

        private void CopyRgb_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(RgbText.Text))
            {
                Clipboard.SetText(RgbText.Text);
            }
        }

        private void CopyHex_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(HexText.Text))
            {
                Clipboard.SetText(HexText.Text);
            }
        }
    }
}
