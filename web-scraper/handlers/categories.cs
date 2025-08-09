using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace WebScraper
{
    public partial class MainWindow
    {
        string? SelectedCategory { get; set; }
        //....................................................................
        private void ProcessCulNetCategory(string CategoryName, int Count=1000)
        {
            DoLog($"Processed Category: {CategoryName}");
        }
        //.....................................................................

        //.....................................................................
        private void CategorySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var X = ComboBox_Categories.SelectedItem;

            if (X is null)
            {
                return;
            }

            if (ComboBox_Categories.SelectedItem is string selectedCategory)
            {
                SelectedCategory = (string)ComboBox_Categories.SelectedItem;
            }
        }
        //.....................................................................

        //.....................................................................
        private void Category_Click(object sender, RoutedEventArgs e)
        {
            Links = new List<string>();

            ListBoxLinks.Items.Clear();

            if (!string.IsNullOrEmpty(SelectedCategory))
            { 
                ProcessCulNetCategory(SelectedCategory);
            }
        }
        //.....................................................................

        //.....................................................................
    }
}
