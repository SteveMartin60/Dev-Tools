using DirectShowLib;
using OpenCvSharp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfCameraApp
{
    public partial class MainWindow : System.Windows.Window
    {
        private VideoCapture _capture;
        private Thread _cameraThread;
        private bool _isCameraRunning;
        private WriteableBitmap _writeableBitmap;
        private readonly List<CameraResolution> _availableResolutions = new List<CameraResolution>();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ResolutionComboBox.ItemsSource = _availableResolutions;
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsCameraAvailable())
            {
                MessageBox.Show("No camera detected or camera access denied");
                return;
            }

            if (_isCameraRunning)
            {
                await StopCameraAsync();

                return;
            }

            StartButton.IsEnabled = false;
            try
            {
                List<CameraResolution> resolutions;
                try
                {
                    resolutions = await Task.Run(() => DirectShowHelper.GetCameraResolutions(0));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to get camera resolutions: {ex.Message}",
                                  "Camera Error",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Error);
                    return;
                }

                Dispatcher.Invoke(() =>
                {
                    _availableResolutions.Clear();
                    _availableResolutions.AddRange(resolutions);
                    ResolutionComboBox.Items.Refresh();
                    ResolutionComboBox.SelectedIndex = 0;
                });

                await InitializeCamera(resolutions[0]);

                if (_capture == null || !_capture.IsOpened())
                {
                    return;
                }

                _isCameraRunning = true;
                StartButton.Content = "Stop";
                _cameraThread = new Thread(CaptureCameraCallback);
                _cameraThread.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error initializing camera: {ex.Message}",
                              "Camera Error",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
            finally
            {
                StartButton.IsEnabled = true;
            }
        }

        private async Task InitializeCamera(CameraResolution resolution)
        {
            await Task.Run(() =>
            {
                _capture = new VideoCapture(0, VideoCaptureAPIs.DSHOW);
                if (!_capture.IsOpened())
                {
                    Dispatcher.Invoke(() => MessageBox.Show("Failed to open camera."));
                    return;
                }

                _capture.Set(VideoCaptureProperties.FrameWidth, resolution.Width);
                _capture.Set(VideoCaptureProperties.FrameHeight, resolution.Height);
            });
        }

        private async Task StopCameraAsync()
        {
            if (!_isCameraRunning) return;

            _isCameraRunning = false;

            // Wait for camera thread to finish
            await Task.Run(() => _cameraThread?.Join(TimeSpan.FromSeconds(2)));

            // Release camera resources
            await Task.Run(() =>
            {
                _capture?.Release();
                _capture = null;
            });

            // Update UI on dispatcher thread
            await Dispatcher.InvokeAsync(() =>
            {
                StartButton.Content = "Start";
                CameraImage.Source = null;
                StartButton.IsEnabled = true;
            });
        }

        private void CaptureCameraCallback()
        {
            using (var frame = new Mat())
            {
                while (_isCameraRunning)
                {
                    if (_capture == null || !_capture.Read(frame) || frame.Empty())
                    {
                        continue;
                    }

                    Dispatcher.Invoke(() =>
                    {
                        UpdateCameraImage(frame);
                    });

                    Thread.Sleep(33); // ~30 FPS
                }
            }
        }

        private void UpdateCameraImage(Mat frame)
        {
            if (_writeableBitmap == null ||
                _writeableBitmap.PixelWidth != frame.Width ||
                _writeableBitmap.PixelHeight != frame.Height)
            {
                _writeableBitmap = new WriteableBitmap(
                    frame.Width,
                    frame.Height,
                    96,
                    96,
                    PixelFormats.Bgr24,
                    null);
                CameraImage.Source = _writeableBitmap;
            }

            _writeableBitmap.Lock();
            unsafe
            {
                Buffer.MemoryCopy(
                    (void*)frame.Data,
                    (void*)_writeableBitmap.BackBuffer,
                    frame.Height * _writeableBitmap.BackBufferStride,
                    frame.Height * frame.Step());
            }
            _writeableBitmap.AddDirtyRect(new Int32Rect(0, 0, frame.Width, frame.Height));
            _writeableBitmap.Unlock();
        }

        private void ResolutionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ResolutionComboBox.SelectedItem is CameraResolution selectedResolution &&
                _capture != null &&
                _capture.IsOpened())
            {
                try
                {
                    _capture.Set(VideoCaptureProperties.FrameWidth, selectedResolution.Width);
                    _capture.Set(VideoCaptureProperties.FrameHeight, selectedResolution.Height);
                    StatusText.Text = $"Resolution set to {selectedResolution}";
                }
                catch
                {
                    StatusText.Text = "Failed to change resolution";
                }
            }
        }

        private void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isCameraRunning)
            {
                MessageBox.Show("Camera is not running.");
                return;
            }

            try
            {
                using (var frame = new Mat())
                {
                    if (_capture != null && _capture.Read(frame) && !frame.Empty())
                    {
                        string fileName = $"capture_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
                        frame.SaveImage(fileName);
                        StatusText.Text = $"Frame saved as {fileName}";
                    }
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Capture failed: {ex.Message}";
            }
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
            await StopCameraAsync();
            Application.Current.Shutdown();
        }

        protected async override void OnClosed(EventArgs e)
        {
            await StopCameraAsync();
            base.OnClosed(e);
        }

        private async void StopButton_Click(object sender, RoutedEventArgs e)
        {
            await StopCameraAsync();
        }

        private bool IsCameraAvailable()
        {
            try
            {
                return DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice).Length > 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
