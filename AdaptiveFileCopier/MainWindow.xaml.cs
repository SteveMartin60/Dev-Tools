using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace FastFileCopier
{
    public partial class MainWindow : Window
    {
        // Hardcoded paths (replace with your actual paths)
        private const string SourceFile = @"E:\AI\Uncensored\Stuff\DeepSeek\DeepSeek-R1-Distill-Qwen-32B-Uncensored\DeepSeek-R1-Distill-Qwen-32B-Uncensored-Q4_k_m.gguf";
        private const string DestFolder = @"M:\AI\Uncensored\Stuff\DeepSeek\DeepSeek-R1-Distill-Qwen-32B-Uncensored\";

        private bool _isCopying;
        private long _detectedCacheSize;
        private Stopwatch _totalStopwatch = new Stopwatch();

        public MainWindow()
        {
            InitializeComponent();
            SourcePathText.Text = SourceFile;
            DestPathText.Text = DestFolder;
        }

        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isCopying) return;

            _isCopying = true;
            CopyButton.IsEnabled = false;

            string destPath = Path.Combine(DestFolder, Path.GetFileName(SourceFile));

            try
            {
                var fileInfo = new FileInfo(SourceFile);
                long fileSize = fileInfo.Length;

                // Detect cache size first
                CacheText.Text = "Detecting cache...";
                _detectedCacheSize = await DetectCacheSizeAsync(Path.GetPathRoot(destPath));
                CacheText.Text = $"Detected cache: {FormatBytes(_detectedCacheSize)}";

                // Start copying
                await CopyFileWithProgressAsync(SourceFile, destPath, fileSize);

                MessageBox.Show("Copy completed successfully!", "Done", MessageBoxButton.OK);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK);
            }
            finally
            {
                _isCopying = false;
                CopyButton.IsEnabled = true;
            }
        }

        private async Task<long> DetectCacheSizeAsync(string drivePath)
        {
            const int sampleSizeMB = 512; // 512MB test chunks
            string testFile = Path.Combine(drivePath, "~cachetest.tmp");

            try
            {
                using (var fs = new FileStream(testFile, FileMode.Create, FileAccess.Write,
                       FileShare.None, 1 * 1024 * 1024, FileOptions.WriteThrough))
                {
                    byte[] buffer = new byte[sampleSizeMB * 1024 * 1024];
                    new Random().NextBytes(buffer);

                    Stopwatch chunkStopwatch = new Stopwatch();
                    double initialSpeed = 0;

                    // Test up to 20% of drive space
                    long maxTestSize = new DriveInfo(drivePath).AvailableFreeSpace / 5;
                    long totalWritten = 0;

                    while (totalWritten < maxTestSize)
                    {
                        chunkStopwatch.Restart();
                        await fs.WriteAsync(buffer, 0, buffer.Length);
                        await fs.FlushAsync();
                        chunkStopwatch.Stop();

                        double currentSpeedMBps = (sampleSizeMB / chunkStopwatch.Elapsed.TotalSeconds);

                        if (totalWritten == 0)
                        {
                            initialSpeed = currentSpeedMBps;
                        }
                        else if (currentSpeedMBps < initialSpeed * 0.5) // 50% speed drop
                        {
                            return totalWritten;
                        }

                        totalWritten += buffer.Length;
                        await Dispatcher.InvokeAsync(() =>
                        {
                            CacheText.Text = $"Testing cache: {FormatBytes(totalWritten)}";
                        });
                    }
                    return maxTestSize; // Return max tested size if no drop detected
                }
            }
            finally
            {
                if (File.Exists(testFile)) File.Delete(testFile);
            }
        }

        private async Task CopyFileWithProgressAsync(string source, string dest, long totalSize)
        {
            int optimalChunkSize = (int)Math.Min(_detectedCacheSize * 0.8, 256 * 1024 * 1024); // Max 256MB
            byte[] buffer = new byte[optimalChunkSize];

            using (var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read))
            using (var destStream = new FileStream(dest, FileMode.Create, FileAccess.Write))
            {
                _totalStopwatch.Restart();
                long totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                {
                    Stopwatch chunkStopwatch = Stopwatch.StartNew();
                    await destStream.WriteAsync(buffer, 0, bytesRead);
                    await destStream.FlushAsync(); // Force commit
                    chunkStopwatch.Stop();

                    totalBytesRead += bytesRead;

                    // Calculate metrics
                    double currentSpeedMBps = (bytesRead / (1024.0 * 1024)) / chunkStopwatch.Elapsed.TotalSeconds;
                    double progressPercentage = (double)totalBytesRead / totalSize * 100;
                    TimeSpan remaining = TimeSpan.FromSeconds(
                        (totalSize - totalBytesRead) / (currentSpeedMBps * 1024 * 1024));

                    // Update UI
                    await Dispatcher.InvokeAsync(() =>
                    {
                        ProgressBar.Value = progressPercentage;
                        ProgressText.Text = $"{progressPercentage:0.0}% ({FormatBytes(totalBytesRead)} / {FormatBytes(totalSize)})";
                        SpeedText.Text = $"{currentSpeedMBps:0.0} MB/s";
                        CopiedText.Text = FormatBytes(totalBytesRead);
                        TimeRemainingText.Text = $"{remaining:mm\\:ss} remaining";

                        // Dynamic chunk adjustment feedback
                        if (currentSpeedMBps < 100) // Slowdown detected
                        {
                            optimalChunkSize = Math.Max(optimalChunkSize / 2, 64 * 1024 * 1024); // Min 64MB
                            buffer = new byte[optimalChunkSize];
                            CacheText.Text = $"Cache recovery (chunk: {FormatBytes(optimalChunkSize)})";
                        }
                        else if (currentSpeedMBps > 500) // Healthy speed
                        {
                            optimalChunkSize = (int)Math.Min(optimalChunkSize * 1.2, _detectedCacheSize);
                            buffer = new byte[optimalChunkSize];
                        }
                    });
                }
                _totalStopwatch.Stop();
            }
        }

        private string FormatBytes(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double number = bytes;

            while (number >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                number /= 1024;
                suffixIndex++;
            }

            return $"{number:0.##} {suffixes[suffixIndex]}";
        }
    }
}