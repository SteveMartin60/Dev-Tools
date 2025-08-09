using IconsRestorer.Code;
using IconsRestorer.ViewModels;
using System.Windows;

namespace IconsRestorer.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = new MainViewModel();
        }
    }
}
