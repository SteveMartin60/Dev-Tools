using Microsoft.Win32;
using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FastFileCopier
{
    public partial class MainWindow : Window
    {
        // Native methods for fast file copy
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CopyFileEx(
            string lpExistingFileName,
            string lpNewFileName,
            CopyProgressRoutine lpProgressRoutine,
            IntPtr lpData,
            ref bool pbCancel,
            int dwCopyFlags);

        private delegate CopyProgressResult CopyProgressRoutine(
            long totalFileSize,
            long totalBytesTransferred,
            long streamSize,
            long streamBytesTransferred,
            uint dwStreamNumber,
            CopyProgressCallbackReason dwCallbackReason,
            IntPtr File,
            IntPtr hDestinationFile,
            IntPtr lpData);

        private enum CopyProgressResult : uint
        {
            PROGRESS_CONTINUE = 0,
            PROGRESS_CANCEL = 1,
            PROGRESS_STOP = 2,
            PROGRESS_QUIET = 3
        }

        private enum CopyProgressCallbackReason : uint
        {
            CALLBACK_CHUNK_FINISHED = 0x00000000,
            CALLBACK_STREAM_SWITCH = 0x00000001
        }

        private const int COPY_FILE_NO_BUFFERING = 0x00000008;
        private const int COPY_FILE_RESTARTABLE = 0x00000002;

        private bool _isCopying;
        private bool _cancelRequested;
        private DispatcherTimer _uiUpdateTimer = new DispatcherTimer();
        private SpeedMetrics _speedMetrics;
        private FileInfo _currentFileInfo;
        DispatcherTimer _timer;

        private class SpeedMetrics
        {
            public long TotalBytesCopied { get; set; }
            public long LastBytesCopied { get; set; }
            public DateTime LastUpdateTime { get; set; }
            public double InstantaneousSpeed { get; set; }
            public double AverageSpeed { get; set; }
            public Stopwatch SpeedTimer { get; } = new Stopwatch();
            public long TotalFileSize { get; set; }

            public void StartTimer() => SpeedTimer.Start();
            public void StopTimer() => SpeedTimer.Stop();
            public void ResetTimer() => SpeedTimer.Reset();
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeTimer();
            InitializeUiUpdateTimer();
        }

        private void InitializeTimer()
        {
            _timer = new System.Windows.Threading.DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1); // Update every 1 second
            _timer.Tick += Timer_Tick; // Hook up the Tick event
            _timer.Start(); // Start the timer
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Your timer logic here (e.g., updating UI, checking file changes, etc.)
            if (_currentFileInfo != null && _currentFileInfo.Exists)
            {
                // Example: Update a label with the last modified time
                //LastModifiedLabel.Content = $"Last Modified: {_currentFileInfo.LastWriteTime}";
            }
        }

        private void InitializeUiUpdateTimer()
        {
            _uiUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _uiUpdateTimer.Tick += UiUpdateTimer_Tick;
        }

        private void UiUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (_isCopying && _speedMetrics != null)
            {
                UpdateProgressDisplay();
            }
        }

        private void BrowseSource_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Source File",
                Filter = "All files (*.*)|*.*",
                CheckFileExists = true,
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                SourcePathTextBox.Text = dialog.FileName;
                _currentFileInfo = new FileInfo(dialog.FileName); // Fixed: Added 'new' keyword
            }
        }

        private void BrowseDestFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select Destination Folder",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                DestFolderTextBox.Text = dialog.FolderName;
            }
        }

        private async void StartCopy_Click(object sender, RoutedEventArgs e)
        {
            if (_isCopying)
            {
                MessageBox.Show("A copy operation is already in progress.",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string sourceFile = SourcePathTextBox.Text;
            string destFolder = DestFolderTextBox.Text;

            if (string.IsNullOrWhiteSpace(sourceFile) || !File.Exists(sourceFile))
            {
                MessageBox.Show("Please select a valid source file.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (string.IsNullOrWhiteSpace(destFolder))
            {
                MessageBox.Show("Please select a destination folder.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!Directory.Exists(destFolder))
            {
                try
                {
                    Directory.CreateDirectory(destFolder);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not create destination folder: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            string destFile = Path.Combine(destFolder, Path.GetFileName(sourceFile));

            if (File.Exists(destFile))
            {
                var result = MessageBox.Show("A file with this name already exists in the destination folder. Overwrite?",
                    "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            try
            {
                PrepareForCopy();
                StatusText.Text = $"Copying {Path.GetFileName(sourceFile)}...";

                if (_currentFileInfo.Length > 100 * 1024 * 1024)
                {
                    await Task.Run(() => NativeCopyWithProgress(sourceFile, destFile));
                }
                else
                {
                    await ManagedCopyWithProgressAsync(sourceFile, destFile);
                }

                if (!_cancelRequested)
                {
                    StatusText.Text = "Copy completed successfully!";
                    MessageBox.Show("File copied successfully!",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                StatusText.Text = $"Error: {ex.Message}";
                MessageBox.Show($"Error copying file: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                CleanupAfterCopy();
            }
        }

        private void NativeCopyWithProgress(string source, string dest)
        {
            bool cancel = false;
            CopyProgressRoutine callback = (total, transferred, streamSize, streamTransferred, streamNumber, reason, src, destPtr, data) =>
            {
                _speedMetrics.TotalBytesCopied = transferred;
                CalculateSpeeds(transferred);
                return _cancelRequested ? CopyProgressResult.PROGRESS_CANCEL : CopyProgressResult.PROGRESS_CONTINUE;
            };

            if (!CopyFileEx(source, dest, callback, IntPtr.Zero, ref cancel, COPY_FILE_NO_BUFFERING | COPY_FILE_RESTARTABLE))
            {
                int error = Marshal.GetLastWin32Error();
                Dispatcher.Invoke(() =>
                {
                    StatusText.Text = $"Copy failed (Error {error})";
                    MessageBox.Show($"Native copy failed with error code: {error}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }

        private async Task ManagedCopyWithProgressAsync(string source, string dest)
        {
            long totalBytes = _currentFileInfo.Length;
            long bytesCopied = 0;
            int bufferSize = Math.Min(4 * 1024 * 1024, (int)(totalBytes / 100));
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

            try
            {
                using (var sourceStream = new FileStream(
                    source,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize,
                    FileOptions.SequentialScan | FileOptions.Asynchronous))
                using (var destStream = new FileStream(
                    dest,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize,
                    FileOptions.WriteThrough | FileOptions.Asynchronous))
                {
                    int cooldownCounter = 0;

                    while (true)
                    {
                        if (_cancelRequested) break;

                        int bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        await destStream.WriteAsync(buffer, 0, bytesRead);
                        bytesCopied += bytesRead;
                        _speedMetrics.TotalBytesCopied = bytesCopied;
                        CalculateSpeeds(bytesCopied);

                        if (++cooldownCounter >= (10 * 1024 * 1024 / bufferSize))
                        {
                            cooldownCounter = 0;
                            await Task.Delay(1);
                            await destStream.FlushAsync();
                        }
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private void CalculateSpeeds(long bytesCopied)
        {
            DateTime now = DateTime.Now;
            double elapsedSeconds = _speedMetrics.SpeedTimer.Elapsed.TotalSeconds;

            if (_speedMetrics.LastUpdateTime != DateTime.MinValue)
            {
                double timeDiff = (now - _speedMetrics.LastUpdateTime).TotalSeconds;
                if (timeDiff > 0)
                {
                    long bytesDiff = bytesCopied - _speedMetrics.LastBytesCopied;
                    _speedMetrics.InstantaneousSpeed = bytesDiff / timeDiff;
                }
            }

            if (elapsedSeconds > 0)
            {
                _speedMetrics.AverageSpeed = bytesCopied / elapsedSeconds;
            }

            _speedMetrics.LastBytesCopied = bytesCopied;
            _speedMetrics.LastUpdateTime = now;
        }

        private void UpdateProgressDisplay()
        {
            try
            {
                long bytesCopied = _speedMetrics.TotalBytesCopied;
                long totalBytes = _speedMetrics.TotalFileSize;

                double progress = (double)bytesCopied / totalBytes * 100;
                ProgressBar.Value = progress;
                ProgressText.Text = $"{progress:F2}% complete";

                InstantSpeedText.Text = $"Current: {FormatSpeed(_speedMetrics.InstantaneousSpeed)}";
                AverageSpeedText.Text = $"Average: {FormatSpeed(_speedMetrics.AverageSpeed)}";
                DataTransferredText.Text = $"{FormatSize(bytesCopied)} of {FormatSize(totalBytes)}";

                if (_speedMetrics.AverageSpeed > 0)
                {
                    double remainingBytes = totalBytes - bytesCopied;
                    double secondsRemaining = remainingBytes / _speedMetrics.AverageSpeed;
                    TimeRemainingText.Text = $"Remaining: {FormatTime(secondsRemaining)}";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Progress update error: {ex.Message}");
            }
        }

        private void PrepareForCopy()
        {
            _isCopying = true;
            _cancelRequested = false;
            _speedMetrics = new SpeedMetrics
            {
                TotalFileSize = _currentFileInfo.Length,
                LastUpdateTime = DateTime.Now // Initialize this to avoid null reference
            };
            _speedMetrics.ResetTimer();
            _speedMetrics.StartTimer();
            StartCopyButton.IsEnabled = false;
            CancelButton.IsEnabled = true;
            _uiUpdateTimer.Start(); // Start the UI update timer
        }

        private void CleanupAfterCopy()
        {
            _isCopying = false;
            _speedMetrics?.StopTimer();
            StartCopyButton.IsEnabled = true;
            CancelButton.IsEnabled = false;
            _uiUpdateTimer.Stop();
        }

        private static string FormatSize(long bytes)
        {
            if (bytes < 0) return "0 B";

            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            double len = bytes;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private static string FormatSpeed(double bytesPerSecond)
        {
            if (bytesPerSecond <= 0) return "0 B/s";

            if (bytesPerSecond > 1024 * 1024 * 1024)
                return $"{bytesPerSecond / (1024 * 1024 * 1024):0.##} GB/s";
            if (bytesPerSecond > 1024 * 1024)
                return $"{bytesPerSecond / (1024 * 1024):0.##} MB/s";
            if (bytesPerSecond > 1024)
                return $"{bytesPerSecond / 1024:0.##} KB/s";
            return $"{bytesPerSecond:0.##} B/s";
        }

        private static string FormatTime(double seconds)
        {
            if (seconds <= 0) return "Calculating...";

            if (seconds > 3600)
                return $"{seconds / 3600:0.##} hours";
            if (seconds > 60)
                return $"{seconds / 60:0.##} minutes";
            return $"{seconds:0.##} seconds";
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isCopying)
            {
                _cancelRequested = true;
                StatusText.Text = "Cancelling...";
                CancelButton.IsEnabled = false;
            }
        }
    }
}
