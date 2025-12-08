using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace reddit_fetch
{
    /// <summary>
    /// WPF Application entrypoint.
    /// If launched with command-line args -> run CLI and exit.
    /// If launched with no args -> show MainWindow (GUI mode).
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var config = LoadConfig();

            CliHandler.Config = config;
            RedditApiHelper.Config = config;
            HashDatabaseHelper.Config = config;
            ImageFilterHelper.Config = config;

            if (e.Args != null && e.Args.Length > 0)
            {
                // Ensure we have a visible console for CLI mode.
                ConsoleWindowHelper.EnsureConsole();

                try
                {
                    var exitCode = CliHandler.HandleAsync(e.Args).GetAwaiter().GetResult();
                    Shutdown(exitCode);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Fatal CLI error: {ex}");
                    Shutdown(1);
                }

                return;
            }

            var mainWindow = new MainWindow();
            MainWindow = mainWindow;
            mainWindow.Show();
        }

        private static AppConfig LoadConfig()
        {
            var config = new AppConfig();

            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var configDir = Path.Combine(appData, "reddit-fetch");
            Directory.CreateDirectory(configDir);

            config.ConfigFilePath = Path.Combine(configDir, "config.json");
            config.Load();

            return config;
        }

        private static class ConsoleWindowHelper
        {
            private const int ATTACH_PARENT_PROCESS = -1;

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool AllocConsole();

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool AttachConsole(int dwProcessId);

            public static void EnsureConsole()
            {
                // Try to attach to an existing console (e.g., when run from cmd).
                if (!AttachConsole(ATTACH_PARENT_PROCESS))
                {
                    // If none, create a new one (e.g., when launched from VS/Explorer).
                    AllocConsole();
                }
            }
        }
    }
}
