using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WebScraper
{
    public partial class MainWindow
    {
        string? SelectedChannel  { get; set; }
        //.....................................................................
        private void ProcessCulNetChannel(string ChannelName, int Count = 1000)
        {
            DoLog($"Processed Category: {ChannelName}");
        }
        //.....................................................................

        //.....................................................................
        private void ChannelSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var X = ComboBox_Channels.SelectedItem;

            if (X is null)
            {
                return;
            }

            if (ComboBox_Channels.SelectedItem is string selectedSearchTerm)
            {
                SelectedChannel = (string)ComboBox_Channels.SelectedItem;
            }
        }
        //.....................................................................

        //.....................................................................
        private List<string> ProcessChannel()
        {
            var Links = new List<string>();

            ListBoxLinks.Items.Clear();

            if (!string.IsNullOrEmpty(SelectedChannel))
            {
                ProcessCulNetChannel(SelectedChannel);
            }

            return Links;
        }

        //.....................................................................

        //.....................................................................
        private void Channel_Click(object sender, RoutedEventArgs e)
        {
            ProcessChannel();
        }
        //.....................................................................

        //.....................................................................
    }
}
