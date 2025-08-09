using System.Globalization;
using System.Threading;
using System.Windows;

namespace IconsRestorer
{
    public partial class App : Application
    {
        public App()
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.CreateSpecificCulture(CultureInfo.CurrentCulture.Name);
        }
    }
}
