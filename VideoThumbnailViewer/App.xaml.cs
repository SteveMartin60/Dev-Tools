using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace VideoThumbnailViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Debug.WriteLine($"Unhandled exception: {args.ExceptionObject}");
            };

            DispatcherUnhandledException += (sender, args) =>
            {
                Debug.WriteLine($"Dispatcher unhandled exception: {args.Exception}");
                args.Handled = true;
            };
        }
    }

}
