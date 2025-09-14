using System.Windows;
using PasswordGenerator.ViewModels; // Add this using directive

namespace PasswordGenerator.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Create an instance of the ViewModel
            var viewModel = new MainViewModel();
            // Set the DataContext of the Window to the ViewModel instance
            this.DataContext = viewModel;
        }
    }
}
