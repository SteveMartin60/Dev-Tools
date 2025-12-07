using System.Windows;

namespace RedditWpf {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            Browser.Navigate("https://www.reddit.com");
        }
    }
}
