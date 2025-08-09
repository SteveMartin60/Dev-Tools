using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WebScraper
{
    public partial class MainWindow
    {
        //.....................................................................
        //.....................................................................
        private void ProcessCulNetSearchTerm(string SearchTerm, int Count = 1000)
        {
            DoLog($"Processed Category: {SearchTerm}");
        }
        //.....................................................................

        //.....................................................................
        private void SearchSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var X = ComboBox_Search.SelectedItem;

            if (X is null)
            {
                return;
            }

            if (ComboBox_Search.SelectedItem is string selectedSearchTerm)
            {
                SelectedSearchTerm = (string)ComboBox_Search.SelectedItem;
            }
        }
        //.....................................................................

        //.....................................................................
        private async void Search_Click(object sender, RoutedEventArgs e)
        {
            Links = new List<string>();

            ListBoxLinks.Items.Clear();

            ProcessCulNetSearchTerm(SelectedSearchTerm);
        }
        //.....................................................................

        //.....................................................................
    }
}
