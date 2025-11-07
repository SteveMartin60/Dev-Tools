using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using WindowTopmostToggler.Services;

namespace WindowTopmostToggler.Models
{
    public class WindowEntry : INotifyPropertyChanged
    {
        public IntPtr Handle { get; }
        public string Title { get; }
        public int ProcessId { get; }
        public string ProcessName { get; }
        public string ClassName { get; }
        public string HandleHex => $"0x{Handle.ToInt64():X}";

        public string ProcessDisplay => $"{ProcessName} (PID {ProcessId})";

        public WindowEntry(IntPtr handle, string title, int pid, string processName, string className)
        {
            Handle = handle;
            Title = title;
            ProcessId = pid;
            ProcessName = processName;
            ClassName = className;
        }

        public bool IsTopmost
        {
            get => Win32.IsWindowTopMost(Handle);
            set
            {
                bool ok = Win32.SetTopMost(Handle, value);
                if (ok) OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}