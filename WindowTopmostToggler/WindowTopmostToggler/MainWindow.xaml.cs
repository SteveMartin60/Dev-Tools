using System.Windows;
using WindowTopmostToggler.ViewModels;

namespace WindowTopmostToggler
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _vm = new MainViewModel();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _vm;
            _vm.Refresh();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            _vm.Refresh();
        }
    }
}