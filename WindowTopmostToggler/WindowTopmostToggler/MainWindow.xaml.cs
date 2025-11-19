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

            Width   = 1350;
            Height  = 850;
            Topmost = true;
            DataContext = _vm;
            _vm.Refresh();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            _vm.Refresh();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            Title = $"Window Topmost Toggler - {Width}x{Height}";
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Title = $"Window Topmost Toggler - {Width}x{Height}";
        }
    }
}