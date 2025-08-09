using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WebScraper
{
    public partial class MainWindow
    {
        //.....................................................................
        TagEntry       SelectedTag { get; set; } = new TagEntry();
        //.....................................................................
        private void ProcessCulNetTag(TagEntry Tag, int Count = 1000)
        {
            DoLog($"Processed Tag: {Tag}");
        }
        //.....................................................................

        //.....................................................................
        public static List<TagEntry> ParseTagFile(string filePath)
        {
            var tagEntries = new List<TagEntry>();

            foreach (var line in File.ReadLines(filePath))
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                // Split by tabs and remove empty entries
                var parts = line.Split('\t')
                                .Where(p => !string.IsNullOrWhiteSpace(p))
                                .Select(p => p.Trim())
                                .ToArray();

                if (parts.Length >= 3)
                {
                    tagEntries.Add(new TagEntry
                    {
                        Id = parts[0],
                        Url = parts[1],
                        TagName = parts[2]
                    });
                }
            }

            return tagEntries;
        }
        //.....................................................................

        //.....................................................................
        private void TagSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var X = ComboBox_Tags.SelectedItem;

            if (X is null)
            {
                return;
            }

            if (ComboBox_Tags.SelectedItem is TagEntry selectedTag)
            {
                SelectedTag = (TagEntry)ComboBox_Tags.SelectedItem;
            }
        }
        //.....................................................................

        //.....................................................................
        private async void Tag_Click(object sender, RoutedEventArgs e)
        {
            Links = new List<string>();

            ListBoxLinks.Items.Clear();

            ProcessCulNetTag(SelectedTag);

            //for (int i = 0; i < Tags.Count; i++)
            //{
            //    ProcessCulNetTag(Tags[i]);
            //}
        }
        //.....................................................................

        //.....................................................................
    }
}
