using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using DrawingColor = System.Drawing.Color;
using DrawingPoint = System.Drawing.Point;
using DrawingBitmap = System.Drawing.Bitmap;
using DrawingGraphics = System.Drawing.Graphics;
using DrawingSize = System.Drawing.Size;

namespace ScreenColorPicker
{
    public partial class PickerOverlay : Window
    {
        public event Action<DrawingColor>? ColorPicked;
        public event Action<DrawingColor>? LiveColorChanged;

        // ---- Magnifier settings ----
        // Number of source pixels on each side of the square sample region
        private const int MagnifierSize = 15;        // 15x15 pixel grid
        // Scale factor: how many screen pixels each source pixel is displayed as
        private const int ZoomFactor = 10;           // 10x zoom => 150x150 display

        // ---- Keyboard step sizes ----
        private const int FineStep = 1;              // Arrow keys
        private const int JumpStep = 10;             // Ctrl + Arrow

        public PickerOverlay()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.Manual;

            // Cover the entire virtual desktop (all monitors)
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            Cursor = Cursors.Cross;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure we get keyboard events (Esc, arrows)
            Keyboard.Focus(this);

            // ---- Apply magnifier geometry based on MagnifierSize & ZoomFactor ----
            double magnifierDisplaySize = MagnifierSize * ZoomFactor;

            if (PreviewMagnifierBox != null)
            {
                PreviewMagnifierBox.Width = magnifierDisplaySize;
                PreviewMagnifierBox.Height = magnifierDisplaySize;
            }

            if (PreviewColorBox != null)
            {
                PreviewColorBox.Width = magnifierDisplaySize;
            }

            if (PreviewBorder != null)
            {
                // Extra room for swatch + hex text + margins
                PreviewBorder.Width = magnifierDisplaySize + 20;
                PreviewBorder.Height = magnifierDisplaySize + 60;
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            var pos = e.GetPosition(this);
            var screenPoint = PointToScreen(pos);

            var screenPointInt = new DrawingPoint(
                (int)screenPoint.X,
                (int)screenPoint.Y);

            UpdateMagnifier(screenPointInt);

            if (PreviewBorder != null)
            {
                PreviewBorder.Visibility = Visibility.Visible;

                double offsetX = 16;
                double offsetY = 16;

                double x = pos.X + offsetX;
                double y = pos.Y + offsetY;

                if (x + PreviewBorder.Width > Width)
                    x = Width - PreviewBorder.Width - 4;
                if (y + PreviewBorder.Height > Height)
                    y = Height - PreviewBorder.Height - 4;

                Canvas.SetLeft(PreviewBorder, x);
                Canvas.SetTop(PreviewBorder, y);
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);
            var screenPoint = PointToScreen(pos);

            var screenPointInt = new DrawingPoint(
                (int)screenPoint.X,
                (int)screenPoint.Y);

            DrawingColor color = GetColorAtScreenPoint(screenPointInt);

            ColorPicked?.Invoke(color);

            Close();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Esc cancels
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
                return;
            }

            // Arrow keys for fine cursor control
            int step = (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control
                ? JumpStep
                : FineStep;

            int dx = 0;
            int dy = 0;

            switch (e.Key)
            {
                case Key.Left:
                    dx = -step;
                    break;
                case Key.Right:
                    dx = step;
                    break;
                case Key.Up:
                    dy = -step;
                    break;
                case Key.Down:
                    dy = step;
                    break;
                default:
                    return; // leave other keys alone
            }

            e.Handled = true;
            MoveCursorBy(dx, dy);
        }

        private static DrawingColor GetColorAtScreenPoint(DrawingPoint location)
        {
            using var bmp = new DrawingBitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            using (var g = DrawingGraphics.FromImage(bmp))
            {
                g.CopyFromScreen(location, System.Drawing.Point.Empty, new System.Drawing.Size(1, 1));
            }

            return bmp.GetPixel(0, 0);
        }

        private void UpdateMagnifier(DrawingPoint location)
        {
            // Virtual screen bounds (multi-monitor safe)
            int vLeft = (int)SystemParameters.VirtualScreenLeft;
            int vTop = (int)SystemParameters.VirtualScreenTop;
            int vWidth = (int)SystemParameters.VirtualScreenWidth;
            int vHeight = (int)SystemParameters.VirtualScreenHeight;

            int half = MagnifierSize / 2;

            int srcX = location.X - half;
            int srcY = location.Y - half;

            // Clamp region so we don't go off-screen
            if (srcX < vLeft) srcX = vLeft;
            if (srcY < vTop) srcY = vTop;
            if (srcX + MagnifierSize > vLeft + vWidth) srcX = vLeft + vWidth - MagnifierSize;
            if (srcY + MagnifierSize > vTop + vHeight) srcY = vTop + vHeight - MagnifierSize;

            using var bmp = new DrawingBitmap(MagnifierSize, MagnifierSize, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            
            using (var g = DrawingGraphics.FromImage(bmp))
            {
                g.CopyFromScreen(srcX, srcY, 0, 0, new DrawingSize(MagnifierSize, MagnifierSize));
            }

            // Center pixel is our current color
            DrawingColor centerColor = bmp.GetPixel(MagnifierSize / 2, MagnifierSize / 2);

            // Raise live event for main window
            LiveColorChanged?.Invoke(centerColor);

            // Update solid swatch + hex in overlay
            var mediaColor = System.Windows.Media.Color.FromArgb(
                    centerColor.A,
                    centerColor.R,
                    centerColor.G,
                    centerColor.B);

            if (PreviewColorBox != null)
            {
                PreviewColorBox.Background =
                    new System.Windows.Media.SolidColorBrush(mediaColor);
            }

            if (PreviewHex != null)
            {
                PreviewHex.Text = $"#{centerColor.R:X2}{centerColor.G:X2}{centerColor.B:X2}";
            }

            // Build BitmapSource for magnifier Image
            if (PreviewMagnifierImage != null)
            {
                IntPtr hBitmap = bmp.GetHbitmap();
                try
                {
                    var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                            hBitmap,
                            IntPtr.Zero,
                            Int32Rect.Empty,
                            BitmapSizeOptions.FromEmptyOptions());

                    PreviewMagnifierImage.Source = source;
                }
                finally
                {
                    DeleteObject(hBitmap);
                }
            }
        }

        private void MoveCursorBy(int dx, int dy)
        {
            if (!GetCursorPos(out POINT current))
            {
                return;
            }

            // Virtual screen bounds
            int vLeft = (int)SystemParameters.VirtualScreenLeft;
            int vTop = (int)SystemParameters.VirtualScreenTop;
            int vWidth = (int)SystemParameters.VirtualScreenWidth;
            int vHeight = (int)SystemParameters.VirtualScreenHeight;

            int newX = current.X + dx;
            int newY = current.Y + dy;

            // Clamp to virtual screen
            if (newX < vLeft) newX = vLeft;
            if (newY < vTop) newY = vTop;
            if (newX > vLeft + vWidth - 1) newX = vLeft + vWidth - 1;
            if (newY > vTop + vHeight - 1) newY = vTop + vHeight - 1;

            SetCursorPos(newX, newY);

            // Update magnifier and live color using the new position
            var p = new DrawingPoint(newX, newY);
            UpdateMagnifier(p);
        }

        // Win32 interop for cursor control + GDI cleanup

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        [DllImport("user32.dll")] private static extern bool GetCursorPos(out POINT lpPoint);
        [DllImport("user32.dll")] private static extern bool SetCursorPos(int X, int Y);
        [DllImport("gdi32.dll" )] private static extern bool DeleteObject(IntPtr hObject);
    }
}
