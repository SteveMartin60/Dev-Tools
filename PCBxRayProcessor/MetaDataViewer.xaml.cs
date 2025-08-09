using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using TagLib;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text;
using File = System.IO.File;

namespace Mp3MetadataViewer
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Mp3Metadata> MetadataCollection { get; set; } = new ObservableCollection<Mp3Metadata>();

        public MainWindow()
        {
            InitializeComponent();
            MetadataDataGrid.ItemsSource = MetadataCollection;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog();
            if (folderDialog.ShowDialog() == true)
            {
                FolderPathTextBox.Text = folderDialog.FolderName;
            }
        }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            string folderPath = FolderPathTextBox.Text;
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("Please select a valid folder path.");
                return;
            }

            MetadataCollection.Clear();

            try
            {
                string[] mp3Files = Directory.GetFiles(folderPath, "*.mp3", SearchOption.AllDirectories);

                foreach (string filePath in mp3Files)
                {
                    try
                    {
                        var Info = new FileInfo(filePath);

                        var file = TagLib.File.Create(filePath);
                        var tag = file.Tag;

                        TimeSpan T = file.Properties.Duration;

                        MetadataCollection.Add(new Mp3Metadata
                        {
                            FileName = Path.GetFileName(filePath),
                            FileSize = $"{Info.Length / 1024 / 1024} MB",
                            Title    = tag.Title,
                            Artist   = tag.FirstPerformer,
                            Album    = tag.Album,
                            Year     = (uint?)tag.Year ?? 0,
                            Track    = (uint?)tag.Track ?? 0,
                            Genre    = tag.FirstGenre,
                            Duration = file.Properties.Duration.ToString(@"mm\:ss"),
                            Seconds  = file.Properties.Duration.TotalSeconds.ToString(@"mm\:ss"),
                            Bitrate  = file.Properties.AudioBitrate + " kbps",
                            Bpm      = tag.BeatsPerMinute
                        });
                    }
                    catch (Exception ex)
                    {
                        // Skip files that can't be read
                        Console.WriteLine($"Error reading {filePath}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error scanning folder: {ex.Message}");
            }
        }
        //.....................................................................

        //.....................................................................
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            if (MetadataCollection.Count == 0)
            {
                MessageBox.Show("No metadata to export. Please scan a folder first.");
                return;
            }

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json|CSV files (*.csv)|*.csv|All files (*.*)|*.*",
                DefaultExt = ".json",
                FileName = "Mp3MetadataExport"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = saveFileDialog.FileName;
                    string directory = Path.GetDirectoryName(filePath);
                    string baseFileName = Path.GetFileNameWithoutExtension(filePath);

                    // Export JSON
                    string jsonPath = Path.Combine(directory, $"{baseFileName}.json");
                    var jsonOptions = new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    };
                    string jsonString = JsonSerializer.Serialize(MetadataCollection, jsonOptions);
                    File.WriteAllText(jsonPath, jsonString);

                    // Export CSV
                    string csvPath = Path.Combine(directory, $"{baseFileName}.csv");
                    var csvContent = new StringBuilder();

                    // Add header row
                    csvContent.AppendLine("FileName,FileSize,Title,Artist,Album,Year,Track,Genre,Duration,Seconds,Bitrate,BPM");

                    // Add data rows
                    foreach (var item in MetadataCollection)
                    {
                        csvContent.AppendLine($"\"{EscapeCsvField(item.FileName)}\"," +
                                            $"\"{EscapeCsvField(item.FileSize)}\"," +
                                            $"\"{EscapeCsvField(item.Title)}\"," +
                                            $"\"{EscapeCsvField(item.Artist)}\"," +
                                            $"\"{EscapeCsvField(item.Album)}\"," +
                                            $"{item.Year}," +
                                            $"{item.Track}," +
                                            $"\"{EscapeCsvField(item.Genre)}\"," +
                                            $"\"{EscapeCsvField(item.Duration)}\"," +
                                            $"\"{EscapeCsvField(item.Seconds)}\"," +
                                            $"\"{EscapeCsvField(item.Bitrate)}\"," +
                                            $"{item.Bpm}");
                    }

                    File.WriteAllText(csvPath, csvContent.ToString());

                    MessageBox.Show($"Successfully exported {MetadataCollection.Count} records to:\n" +
                                  $"- {jsonPath}\n" +
                                  $"- {csvPath}", "Export Complete");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error");
                }
            }
        }
        //.....................................................................

        //.....................................................................
        private string EscapeCsvField(string field)
        {
            if (field == null)
                return string.Empty;

            // Escape quotes by doubling them
            return field.Replace("\"", "\"\"");
        }
        //.....................................................................
    }
    //.........................................................................

    //.........................................................................
    public class Mp3Metadata
    {
        public string FileName { get; set; }
        public string FileSize { get; set; }
        public string Title    { get; set; }
        public string Artist   { get; set; }
        public string Album    { get; set; }
        public uint   Year     { get; set; }
        public uint   Track    { get; set; }
        public string Genre    { get; set; }
        public string Duration { get; set; }
        public string Seconds  { get; set; }
        public string Bitrate  { get; set; }
        public uint   Bpm      { get; set; }
    }
}
