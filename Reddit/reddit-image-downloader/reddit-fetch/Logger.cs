using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reddit_fetch
{
    /// <summary>
    /// Provides logging functionality with support for different output targets and log levels.
    /// </summary>
    public static class Logger
    {
        private static string _logFilePath = "app.log";
        private static int _logLevel = 1; // Default to Information (1)

        public static bool UseConsole { get; set; } = true;
        public static bool UseFile { get; set; } = true;
        public static bool UseGui { get; set; } = false;

        public static Action<string>? GuiLogAction { get; set; }

        public static string LogFilePath
        {
            get => _logFilePath;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Log file path cannot be empty.");

                try
                {
                    var directory = Path.GetDirectoryName(value);

                    if (string.IsNullOrWhiteSpace(directory))
                        throw new ArgumentException("Log file path must include a valid directory.");

                    if (!Directory.Exists(directory))
                        throw new DirectoryNotFoundException($"Directory does not exist: {directory}");

                    using (var stream = new FileStream(value, FileMode.Append, FileAccess.Write))
                    {
                        // If we can open it, path is good.
                    }

                    _logFilePath = value;
                }
                catch (Exception ex)
                {
                    throw new IOException($"Failed to set log file path: {ex.Message}", ex);
                }
            }
        }

        /// <summary>
        /// 0 = Error, 1 = Info, 2 = Debug
        /// </summary>
        public static int LogLevel
        {
            get => _logLevel;
            set
            {
                if (value < 0 || value > 2)
                    throw new ArgumentOutOfRangeException(nameof(value), "Log level must be between 0 and 2.");
                _logLevel = value;
            }
        }

        public static void LogError(string message) => Log(message, 0, "ERROR");
        public static void LogInfo(string message) => Log(message, 1, "INFO");
        public static void LogDebug(string message) => Log(message, 2, "DEBUG");

        private static void Log(string message, int level, string prefix)
        {
            if (level > _logLevel)
                return;

            string timestamped = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{prefix}] {message}";

            if (UseConsole)
            {
                if (level == 0)
                    Console.Error.WriteLine(timestamped);
                else
                    Console.WriteLine(timestamped);
            }

            if (UseFile)
            {
                try
                {
                    File.AppendAllText(_logFilePath, timestamped + Environment.NewLine);
                }
                catch
                {
                    // Swallow file logging failures to avoid crashing
                }
            }

            if (UseGui && GuiLogAction != null)
            {
                GuiLogAction.Invoke(timestamped);
            }
        }
    }
}
