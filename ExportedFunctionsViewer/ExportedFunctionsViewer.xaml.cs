using Microsoft.Win32;
using Native.Windows.Methods;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace ExportedFunctionsViewer
{
    public sealed partial class MainWindow : Window
    {
        private string? _dumpBinPath;
        private readonly ObservableCollection<FileSystemItem> _fileSystemItems = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            _dumpBinPath = FindDumpBinPath();
            Topmost = true;
        }

        public ObservableCollection<FileSystemItem> FileSystemItems => _fileSystemItems;

        private void OnBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Select a folder containing executable files"
            };

            if (dialog.ShowDialog() == true)
            {
                FolderPathTextBox.Text = dialog.FolderName;
                LoadDirectoryContents(dialog.FolderName);
            }
        }

        private void LoadDirectoryContents(string directoryPath)
        {
            _fileSystemItems.Clear();

            try
            {
                foreach (var dir in Directory.GetDirectories(directoryPath))
                {
                    _fileSystemItems.Add(new DirectoryItem
                    {
                        Name = Path.GetFileName(dir),
                        FullPath = dir
                    });
                }

                foreach (var file in Directory.GetFiles(directoryPath, "*.exe")
                    .Concat(Directory.GetFiles(directoryPath, "*.dll")))
                {
                    _fileSystemItems.Add(new ExecutableFileItem
                    {
                        Name = Path.GetFileName(file),
                        FullPath = file
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading directory: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnTreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is ExecutableFileItem fileItem)
            {
                DisplayExportedFunctions(fileItem.FullPath);
            }
        }

        private void DisplayExportedFunctions(string filePath)
        {
            if (string.IsNullOrEmpty(_dumpBinPath))
            {
                MessageBox.Show("dumpbin.exe not found. Please install Visual Studio with C++ components.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var exports = GetExportedFunctions(filePath);
                ExportsDataGrid.ItemsSource = exports;

                if (exports.Count == 0)
                {
                    MessageBox.Show("No exported functions found in the selected file.",
                        "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading exports: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ObservableCollection<ExportedFunction> GetExportedFunctions(string filePath)
        {
            var exports = new ObservableCollection<ExportedFunction>();

            var startInfo = new ProcessStartInfo
            {
                FileName = _dumpBinPath,
                Arguments = $"/exports \"{filePath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            };

            using var process = Process.Start(startInfo);
            try
            {
                process.WaitForExit(5000);
                if (process.ExitCode != 0)
                {
                    throw new Exception($"dumpbin failed with exit code: {process.ExitCode}");
                }

                var output = process.StandardOutput.ReadToEnd();
                ParseExports(output, exports);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error processing dumpbin output: {ex.Message}", ex);
            }

            return exports;
        }

        private void ParseExports(string output, ObservableCollection<ExportedFunction> exports)
        {
            bool inExportsSection = false;
            foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (line.Contains("ordinal") && line.Contains("hint") && line.Contains("RVA") && line.Contains("name"))
                {
                    inExportsSection = true;
                    continue;
                }

                if (inExportsSection)
                {
                    if (line.Contains("Summary")) break;

                    var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 4 && int.TryParse(parts[0], out int ordinal))
                    {
                        string functionName = parts.Length > 4 ? string.Join(" ", parts, 3, parts.Length - 3) : parts[3];
                        exports.Add(new ExportedFunction
                        {
                            Ordinal = ordinal,
                            Hint = parts[1],
                            RVA = parts[2],
                            Name = functionName,
                            DemangledName = DemangleName(functionName)
                        });
                    }
                }
            }
        }

        private string DemangleName(string mangledName)
        {
            try
            {
                if (string.IsNullOrEmpty(mangledName))
                    return mangledName;

                // Handle standard C++ decorated names
                if (mangledName.StartsWith("?"))
                {
                    // Handle constructors/destructors/operators
                    if (mangledName.StartsWith("??0")) return $"Constructor: {DemangleClassName(mangledName.Substring(3))}";
                    if (mangledName.StartsWith("??1")) return $"Destructor: {DemangleClassName(mangledName.Substring(3))}";
                    if (mangledName.StartsWith("??4")) return $"Operator=: {DemangleClassName(mangledName.Substring(3))}";

                    // Handle vtable
                    if (mangledName.StartsWith("??_7")) return $"VTable: {DemangleClassName(mangledName.Substring(4))}";

                    // Handle other special cases
                    if (mangledName.StartsWith("??_F")) return $"Scalar Deleting Destructor: {DemangleClassName(mangledName.Substring(4))}";
                    if (mangledName.StartsWith("??_O")) return $"Operator(): {DemangleClassName(mangledName.Substring(4))}";

                    // Handle regular member functions
                    if (mangledName.Contains("@"))
                    {
                        var parts = mangledName.Split('@');
                        if (parts.Length >= 2)
                        {
                            string className = DemangleClassName(parts[0].TrimStart('?'));
                            string methodName = parts[1].TrimStart('?');

                            if (methodName.Contains("@@"))
                            {
                                var methodParts = methodName.Split(new[] { "@@" }, 2, StringSplitOptions.None);
                                return $"{className}::{methodParts[0]}({DemangleParameters(methodParts[1])})";
                            }

                            return $"{className}::{methodName}";
                        }
                    }
                }

                // Handle regular C-style exports
                if (mangledName.StartsWith("_") || mangledName.StartsWith("@"))
                {
                    string cleanName = mangledName.TrimStart('_', '@');
                    int atPos = cleanName.IndexOf('@');
                    return atPos > 0 ? cleanName.Substring(0, atPos) : cleanName;
                }

                return mangledName;
            }
            catch
            {
                return mangledName;
            }
        }

        private string DemangleClassName(string mangledName)
        {
            if (string.IsNullOrEmpty(mangledName))
                return mangledName;

            int templatePos = mangledName.IndexOf('@');
            return templatePos > 0 ? mangledName.Substring(0, templatePos) : mangledName;
        }

        private string DemangleParameters(string parameters)
        {
            var paramList = new List<string>();
            int pos = 0;

            while (pos < parameters.Length)
            {
                switch (parameters[pos])
                {
                    case 'H':
                        paramList.Add("int");
                        pos++;
                        break;
                    case 'N':
                        paramList.Add("numeric");
                        pos++;
                        break;
                    case 'P':
                        paramList.Add("pointer");
                        pos++;
                        break;
                    case 'D':
                        paramList.Add("double");
                        pos++;
                        break;
                    case 'E':
                        paramList.Add("enum");
                        pos++;
                        break;
                    case 'X':
                        paramList.Add("void");
                        pos++;
                        break;
                    case '_':
                        paramList.Add("special");
                        pos++;
                        break;
                    default:
                        int endPos = parameters.IndexOf('@', pos);
                        if (endPos < 0) break;
                        paramList.Add(DemangleClassName(parameters.Substring(pos, endPos - pos)));
                        pos = endPos + 1;
                        break;
                }
            }

            return string.Join(", ", paramList);
        }

        private string? FindDumpBinPath()
        {
            try
            {
                string vsWherePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                    "Microsoft Visual Studio", "Installer", "vswhere.exe");

                if (File.Exists(vsWherePath))
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = vsWherePath,
                        Arguments = "-latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -find ​**​\\dumpbin.exe",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(psi);
                    string output = process.StandardOutput.ReadToEnd().Trim();
                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(output))
                    {
                        return output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .FirstOrDefault(File.Exists);
                    }
                }

                string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                var vsRoot = Path.Combine(programFiles, "Microsoft Visual Studio", "2022");

                if (Directory.Exists(vsRoot))
                {
                    return Directory.EnumerateDirectories(vsRoot)
                        .Select(edition => Path.Combine(edition, "VC", "Tools", "MSVC"))
                        .Where(Directory.Exists)
                        .SelectMany(msvc => Directory.EnumerateDirectories(msvc)
                            .OrderByDescending(d => d)
                            .Select(version => Path.Combine(version, "bin", "Hostx64", "x64", "dumpbin.exe")))
                        .FirstOrDefault(File.Exists);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error finding dumpbin: {ex.Message}");
            }

            return null;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            NativeMethods.SetTitle(this);
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            NativeMethods.SetTitle(this);
        }
    }

    public abstract class FileSystemItem
    {
        public required string Name { get; init; }
        public required string FullPath { get; init; }
        public bool IsDirectory { get; protected init; }
    }

    public sealed class DirectoryItem : FileSystemItem
    {
        public DirectoryItem() => IsDirectory = true;
    }

    public sealed class ExecutableFileItem : FileSystemItem
    {
        public ExecutableFileItem() => IsDirectory = false;
    }

    public sealed class ExportedFunction
    {
        public int Ordinal { get; set; }
        public string Hint { get; set; } = string.Empty;
        public string RVA { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string DemangledName { get; set; } = string.Empty;
    }
}