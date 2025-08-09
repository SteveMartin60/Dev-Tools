using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LnkEditor
{
    public partial class link_editor : Window
    {
        private void BrowseTarget_Click(object sender, RoutedEventArgs e)
        {
            // TODO: implement
        }

        private void BrowseWorkingDirectory_Click(object sender, RoutedEventArgs e)
        {
            // TODO: implement
        }

        private void BrowseIcon_Click(object sender, RoutedEventArgs e)
        {
            // TODO: implement
        }

        private void PickIconFromDll_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select a DLL or EXE File",
                Filter = "DLL Files (*.dll)|*.dll|Executable Files (*.exe)|*.exe|All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                var icons = ExtractIcons(filePath);

                if (icons.Count > 0)
                {
                    var iconDialog = new Window
                    {
                        Title = "Select an Icon",
                        Width = 600,
                        Height = 400,
                        WindowStartupLocation = WindowStartupLocation.CenterScreen
                    };

                    var wrapPanel = new WrapPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Margin = new Thickness(10)
                    };

                    foreach (var icon in icons)
                    {
                        var image = new System.Windows.Controls.Image
                        {
                            Source = icon,
                            Width = 32,
                            Height = 32,
                            Margin = new Thickness(5)
                        };

                        var button = new Button
                        {
                            Content = image,
                            Tag = icon,
                            Width = 40,
                            Height = 40,
                            Margin = new Thickness(5)
                        };

                        button.Click += (s, args) =>
                        {
                            IconLocation.Text = $"{filePath},{icons.IndexOf(button.Tag as BitmapSource)}";
                            iconDialog.Close();
                        };

                        wrapPanel.Children.Add(button);
                    }

                    iconDialog.Content = new ScrollViewer { Content = wrapPanel };
                    iconDialog.ShowDialog();
                }
                else
                {
                    MessageBox.Show("No icons found in the selected file.", "Info",
                                    MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void Hotkey_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // TODO: implement
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            // TODO: implement
        }
    }
}
