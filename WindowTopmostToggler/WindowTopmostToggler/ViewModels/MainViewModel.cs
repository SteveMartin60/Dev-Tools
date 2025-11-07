using System.Collections.ObjectModel;
using WindowTopmostToggler.Models;
using WindowTopmostToggler.Services;

namespace WindowTopmostToggler.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<WindowEntry> Windows { get; } = new();

        public void Refresh()
        {
            Windows.Clear();
            foreach (var (Handle, Title, Pid, ProcessName, ClassName) in Win32.EnumerateWindows())
            {
                Windows.Add(new WindowEntry(Handle, Title, Pid, ProcessName, ClassName));
            }
        }
    }
}