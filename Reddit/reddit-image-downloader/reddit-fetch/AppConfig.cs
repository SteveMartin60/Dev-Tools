using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace reddit_fetch
{
    /// <summary>
    /// Provides helper methods for retrieving system-specific paths.
    /// </summary>
    public static partial class PathHelper
    {
        public static string GetDefaultDownloadPath()
        {
            try
            {
                return @"D:\TS-RO\Reddit-Fetch";
                //return Path.Combine(
                //    Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                //    "reddit-fetch");
            }
            catch
            {
                return Path.Combine(AppContext.BaseDirectory, "downloads");
            }
        }

        [DllImport("shell32.dll")]
        private static extern int SHGetKnownFolderPath(
            [MarshalAs(UnmanagedType.LPStruct)] Guid rfid,
            uint dwFlags,
            IntPtr hToken,
            out IntPtr ppszPath);
    }

    /// <summary>
    /// Manages loading and saving application configuration settings from a JSON file.
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// The full path to the JSON configuration file.
        /// This should be set externally before calling Load or Save.
        /// </summary>
        public string ConfigFilePath { get; set; } = string.Empty;

        // Settings properties
        public string DatabasePath { set; get; } = Path.Combine(AppContext.BaseDirectory, "image-hashes.db");
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string UserAgent { get; set; } = "windows:reddit-fetch:v1.0 (by github.com/dwhagar)";
        public string DownloadPath { get; set; } = PathHelper.GetDefaultDownloadPath();
        public int MinutesBetweenChecks { get; set; } = 60;

        public List<SubredditInfo> Subreddits { get; set; } = new();

        /// <summary>
        /// Loads the configuration from the JSON file at ConfigFilePath.
        /// If the file does not exist, creates a default configuration.
        /// </summary>
        public void Load()
        {
            if (string.IsNullOrWhiteSpace(ConfigFilePath))
                throw new InvalidOperationException("ConfigFilePath must be set before calling Load().");

            if (!File.Exists(ConfigFilePath))
            {
                Save(); // create default config
                return;
            }

            var json = File.ReadAllText(ConfigFilePath);
            var loaded = JsonSerializer.Deserialize<AppConfig>(json);

            if (loaded == null)
                return;

            DatabasePath = loaded.DatabasePath;
            ClientId = loaded.ClientId;
            ClientSecret = loaded.ClientSecret;
            UserAgent = loaded.UserAgent;
            DownloadPath = loaded.DownloadPath;
            MinutesBetweenChecks = loaded.MinutesBetweenChecks;
            Subreddits = loaded.Subreddits ?? new();
        }

        /// <summary>
        /// Saves the configuration to the JSON file at ConfigFilePath.
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrWhiteSpace(ConfigFilePath))
                throw new InvalidOperationException("ConfigFilePath must be set before calling Save().");

            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            Directory.CreateDirectory(Path.GetDirectoryName(ConfigFilePath)!);
            File.WriteAllText(ConfigFilePath, json);
        }
    }

    /// <summary>
    /// Represents a configured subreddit with per-subreddit state.
    /// </summary>
    public class SubredditInfo
    {
        /// <summary>
        /// The name of the subreddit.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The last time this subreddit was checked for new posts.
        /// </summary>
        public DateTime LastCheckDate { get; set; } = new DateTime(1970, 1, 1);

        /// <summary>
        /// The last post date from this subreddit that was downloaded.
        /// </summary>
        public DateTime LastPostDate { get; set; } = new DateTime(1970, 1, 1);

        /// <summary>
        /// How many posts to attempt to download per check.
        /// </summary>
        public int MaxPostsPerCheck { get; set; } = 100;

        /// <summary>
        /// Validates whether the current subreddit name is acceptable according to basic Reddit naming rules.
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrWhiteSpace(Name))
                return false;

            foreach (char c in Name)
            {
                if (!(char.IsLetterOrDigit(c) || c == '_'))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
