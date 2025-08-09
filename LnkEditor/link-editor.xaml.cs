// MainWindow.xaml.cs
// Full, un-abbreviated code-behind for LnkEditor
// (WPF, no WinForms)

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace LnkEditor
{
    public partial class link_editor : Window
    {
        #region ----------  Constructor  ----------
        public link_editor()
        {
            InitializeComponent();
        }
        #endregion

        private string _currentShortcutPath;

        private void BrowseShortcut_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Select a Windows shortcut",
                Filter = "Shortcuts (*.lnk)|*.lnk",
                Multiselect = false
            };

            if (dlg.ShowDialog() != true) return;

            string lnkPath = dlg.FileName;
            Debug.WriteLine($"\n==== Loading shortcut: {lnkPath} ====");

            try
            {
                // 1. COM Object Creation Diagnostics
                Debug.WriteLine("Creating ShellLink COM object...");
                var shellLink = (IShellLinkW)new ShellLinkCoClass();
                var persistFile = (IPersistFile)shellLink;
                Debug.WriteLine($"COM objects created - ShellLink: {shellLink != null}, PersistFile: {persistFile != null}");

                // 2. File Loading Diagnostics
                Debug.WriteLine("Loading shortcut file...");
                persistFile.Load(lnkPath, 0);
                Debug.WriteLine("File loaded successfully");

                const int bufferSize = 2048;
                var sb = new StringBuilder(bufferSize);

                // 3. Target Path Diagnostics
                Debug.WriteLine("\nGetting Target Path...");
                shellLink.GetPath(sb, bufferSize, IntPtr.Zero, 0);
                Debug.WriteLine($"Raw target path: '{sb.ToString()}' (Length: {sb.Length})");
                TargetPath.Text = sb.Length > 0 ? sb.ToString() : "[Empty]";
                sb.Clear();

                // 4. Arguments Diagnostics
                Debug.WriteLine("\nGetting Arguments...");
                shellLink.GetArguments(sb, bufferSize);
                Debug.WriteLine($"Raw arguments: '{sb.ToString()}' (Length: {sb.Length})");
                Arguments.Text = sb.Length > 0 ? sb.ToString() : "[Empty]";
                sb.Clear();

                // 5. Working Directory Diagnostics
                Debug.WriteLine("\nGetting Working Directory...");
                shellLink.GetWorkingDirectory(sb, bufferSize);
                Debug.WriteLine($"Raw working dir: '{sb.ToString()}' (Length: {sb.Length})");
                WorkingDirectory.Text = sb.Length > 0 ? sb.ToString() : "[Empty]";
                sb.Clear();

                // 6. Icon Location Diagnostics
                Debug.WriteLine("\nGetting Icon Location...");
                shellLink.GetIconLocation(sb, bufferSize, out int iconIndex);
                Debug.WriteLine($"Raw icon location: '{sb.ToString()}', Index: {iconIndex} (Length: {sb.Length})");
                IconLocation.Text = sb.Length > 0 ? $"{sb},{iconIndex}" : "[Empty],0";
                sb.Clear();

                // 7. Hotkey Diagnostics
                Debug.WriteLine("\nGetting Hotkey...");
                shellLink.GetHotkey(out ushort hotKey);
                Debug.WriteLine($"Raw hotkey value: {hotKey} (0x{hotKey:X4})");
                Debug.WriteLine($"Binary: {Convert.ToString(hotKey, 2).PadLeft(16, '0')}");
                Hotkey.Text = hotKey != 0 ? HotKeyToString(hotKey) : "None";

                // 8. Window State Diagnostics
                Debug.WriteLine("\nGetting Window State...");
                shellLink.GetShowCmd(out int showCmd);
                Debug.WriteLine($"Raw showCmd value: {showCmd}");
                RunCombo.SelectedIndex = showCmd switch
                {
                    0 => 0,
                    2 => 2,
                    3 => 3,
                    _ => 1
                };

                // 9. Final COM Cleanup
                Marshal.ReleaseComObject(persistFile);
                Marshal.ReleaseComObject(shellLink);

                _currentShortcutPath = lnkPath;
                ShortcutPath.Text = lnkPath;
                Debug.WriteLine("\n==== Shortcut load completed ====\n");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"\n!!!! EXCEPTION !!!!\n{ex.ToString()}\n");
                MessageBox.Show($"Failed to load shortcut properties:\n{ex.Message}",
                               "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
