using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using CodeConsolidator.Services;
using System.Runtime.InteropServices;
using System.Windows.Documents;

namespace CodeConsolidator
{
    public partial class MainWindow : Window
    {
        private ThemeService _themeService;
        private bool _darkMode = false;
        private ProgressBar _progressBar;
        private SyntaxHighlighter _syntaxHighlighter;
        private string _lastSelectedFolderPath;
        private bool _isPinned = false; // Tracks whether the window is pinned (topmost)
        private const string PinnedIconPath = "icon_233.png";
        private const string UnpinnedIconPath = "icon_228.png";

        public MainWindow()
        {
            InitializeComponent();
            InitializeProgressBar();
            EnsureIconsExist(); // Ensure icons are extracted and saved locally
            InitializePinButtonIcons();
            _syntaxHighlighter = new SyntaxHighlighter(_darkMode);
            _themeService = new ThemeService(this, CodeDisplay, StatusText, TopControlPanel);
        }

        /// <summary>
        /// Converts an icon handle to a BitmapSource for WPF.
        /// </summary>
        private BitmapSource LoadIconFromHandle(IntPtr hIcon)
        {
            if (hIcon == IntPtr.Zero)
            {
                throw new ArgumentException("Icon handle cannot be null or zero.");
            }

            try
            {
                return Imaging.CreateBitmapSourceFromHIcon(
                    hIcon,
                    Int32Rect.Empty, // Use the entire icon
                    BitmapSizeOptions.FromEmptyOptions() // No resizing
                );
            }
            finally
            {
                // Destroy the icon handle to avoid memory leaks
                DestroyIcon(hIcon);
            }
        }

        /// <summary>
        /// Ensures that the required icons exist in the application directory.
        /// If not, extracts them from imageres.dll and saves them locally.
        /// </summary>
        private void EnsureIconsExist()
        {
            if (!File.Exists(PinnedIconPath))
            {
                ExtractAndSaveIcon("imageres.dll", 129, PinnedIconPath); // Pinned icon
            }

            if (!File.Exists(UnpinnedIconPath))
            {
                ExtractAndSaveIcon("imageres.dll", 130, UnpinnedIconPath); // Unpinned icon
            }
        }

        /// <summary>
        /// Extracts an icon from a DLL and saves it to the specified file path.
        /// </summary>
        private void ExtractAndSaveIcon(string dllName, int iconIndex, string outputPath)
        {
            // Load the icon from the DLL
            IntPtr hIcon = LoadImage(IntPtr.Zero, dllName, 1, 0, 0, (uint)(0x00000080 | iconIndex));
            if (hIcon == IntPtr.Zero)
            {
                StatusText.Text = $"Failed to extract icon with index {iconIndex} from {dllName}.";
                return;
            }

            try
            {
                // Convert the icon handle to a System.Drawing.Icon
                using (var icon = System.Drawing.Icon.FromHandle(hIcon))
                {
                    // Save the icon to the specified file path
                    using (var stream = new FileStream(outputPath, FileMode.Create))
                    {
                        icon.Save(stream);
                    }
                }
            }
            finally
            {
                // Clean up the icon handle
                DestroyIcon(hIcon);
            }
        }

        /// <summary>
        /// Initializes the pin button with the default "unpinned" icon.
        /// </summary>
        private void InitializePinButtonIcons()
        {
            // Load the initial "unpinned" icon
            PinIcon.Source = LoadIconFromFile(UnpinnedIconPath);
        }

        /// <summary>
        /// Handles the click event of the pin button.
        /// Toggles the window's Topmost property and updates the icon.
        /// </summary>
        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            _isPinned = !_isPinned;
            this.Topmost = _isPinned;

            // Change the icon based on the pinned state
            PinIcon.Source = _isPinned
                ? LoadIconFromFile(PinnedIconPath) // Pinned icon
                : LoadIconFromFile(UnpinnedIconPath); // Unpinned icon

            StatusText.Text = _isPinned
                ? "Window is now pinned (always on top)."
                : "Window is no longer pinned.";
        }

        /// <summary>
        /// Loads an icon from a local file.
        /// </summary>
        private BitmapSource LoadIconFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                return new BitmapImage(new Uri(filePath, UriKind.RelativeOrAbsolute));
            }

            StatusText.Text = $"Icon file not found: {filePath}";
            return null;
        }

        /// <summary>
        /// Initializes the progress bar control.
        /// </summary>
        private void InitializeProgressBar()
        {
            _progressBar = new ProgressBar
            {
                Height = 5,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(0, 5, 0, 0),
                Visibility = Visibility.Collapsed
            };
            MainGrid.Children.Add(_progressBar);
            Grid.SetRow(_progressBar, 2);
        }

        /// <summary>
        /// Handles the folder selection process.
        /// </summary>
        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new OpenFolderDialog();
            if (folderDialog.ShowDialog() == true)
            {
                ProcessSelectedFolder(folderDialog.FolderName);
            }
        }

        /// <summary>
        /// Processes the selected folder and displays its contents.
        /// </summary>
        private void ProcessSelectedFolder(string folderPath)
        {
            CodeDisplay.Document.Blocks.Clear();
            var filteredFiles = new List<string>();

            try
            {
                _progressBar.Visibility = Visibility.Visible;
                _progressBar.Value = 0;
                StatusText.Text = "Searching files...";
                Application.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background);

                var searchOption = RecursiveSearchCheckBox.IsChecked == true
                    ? SearchOption.AllDirectories
                    : SearchOption.TopDirectoryOnly;

                var allFiles = Directory.GetFiles(folderPath, "*.*", searchOption);
                var temporaryPatterns = new[] { "~$*", "*.tmp", "*.bak", "*.swp" };

                filteredFiles = allFiles.Where(filePath =>
                {
                    var fileName = Path.GetFileName(filePath);
                    var directoryName = Path.GetDirectoryName(filePath);
                    return !temporaryPatterns.Any(pattern => fileName.StartsWith(pattern.Trim('*'))) &&
                           !directoryName.Contains("bin") &&
                           !directoryName.Contains("obj") &&
                           !directoryName.Contains("deprecated") &&
                           !directoryName.Contains("Temp") &&
                           (filePath.EndsWith(".cs") || filePath.EndsWith(".xaml") || filePath.EndsWith(".axaml") || filePath.EndsWith(".sln") || filePath.EndsWith(".csproj"));

                }).ToList();

                _progressBar.Maximum = filteredFiles.Count;
                var flowDocument = new FlowDocument();

                foreach (var filePath in filteredFiles)
                {
                    _progressBar.Value++;
                    StatusText.Text = $"Processing {_progressBar.Value} of {filteredFiles.Count}...";
                    Application.Current.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Background);

                    // Add file header
                    var fileIcon = filePath.EndsWith(".cs") ? "◈" : (filePath.EndsWith(".xaml") || filePath.EndsWith(".xaml")) ? "◇" : "⚙";
                    var headerColor = filePath.EndsWith(".cs") ? Brushes.DodgerBlue : (filePath.EndsWith(".xaml") || filePath.EndsWith(".axaml")) ? Brushes.Orange : Brushes.Green;

                    flowDocument.Blocks.Add(new Paragraph(new Run($"// ======================================================================"))
                    {
                        Foreground = Brushes.Gray
                    });

                    flowDocument.Blocks.Add(new Paragraph(new Run($"{fileIcon} Begin File: {filePath.Substring(folderPath.Length + 1)}"))
                    {
                        Foreground = headerColor,
                        FontWeight = FontWeights.Bold
                    });

                    flowDocument.Blocks.Add(new Paragraph(new Run($"// -----------------------------------"))
                    {
                        Foreground = Brushes.Gray
                    });

                    flowDocument.Blocks.Add(new Paragraph());

                    // Add file content with syntax highlighting
                    var fileContent = File.ReadAllText(filePath);
                    if (filePath.EndsWith(".cs"))
                    {
                        _syntaxHighlighter.HighlightCSharpCode(fileContent, flowDocument);
                    }
                    else if (filePath.EndsWith(".xaml") || filePath.EndsWith(".axaml"))
                    {
                        _syntaxHighlighter.HighlightXamlCode(fileContent, flowDocument);
                    }
                    else if (filePath.EndsWith(".sln"))
                    {
                        _syntaxHighlighter.HighlightSlnCode(fileContent, flowDocument);
                    }
                    else if (filePath.EndsWith(".csproj"))
                    {
                        _syntaxHighlighter.HighlightCsProjCode(fileContent, flowDocument);
                    }

                    // Add file footer
                    flowDocument.Blocks.Add(new Paragraph());
                    flowDocument.Blocks.Add(new Paragraph(new Run($"// -----------------------------------"))
                    {
                        Foreground = Brushes.Gray
                    });

                    flowDocument.Blocks.Add(new Paragraph(new Run($"{fileIcon} End File: {filePath.Substring(folderPath.Length + 1)}"))
                    {
                        Foreground = headerColor,
                        FontWeight = FontWeights.Bold
                    });

                    flowDocument.Blocks.Add(new Paragraph(new Run($"// ======================================================================"))
                    {
                        Foreground = Brushes.Gray
                    });

                    flowDocument.Blocks.Add(new Paragraph());
                }

                CodeDisplay.Document = flowDocument;
                StatusText.Text = $"Loaded {filteredFiles.Count} files ({(RecursiveSearchCheckBox.IsChecked == true ? "recursive" : "non-recursive")})";
                _progressBar.Visibility = Visibility.Collapsed;
                _lastSelectedFolderPath = folderPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing files: {ex.Message}");
                StatusText.Text = $"Error: {ex.Message} (loaded {filteredFiles.Count} files)";
                _progressBar.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Exports the displayed code to a text file.
        /// </summary>
        private void ExportCodeButton_Click(object sender, RoutedEventArgs e)
        {
            var baseFolderName = _lastSelectedFolderPath != null
                ? Path.GetFileName(_lastSelectedFolderPath)
                : "ExportedCode";

            var recursiveIndicator = RecursiveSearchCheckBox.IsChecked == true
                ? "_Recursive"
                : "_NonRecursive";

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var defaultFileName = $"{baseFolderName}-code-export.txt";

            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = defaultFileName
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    var textRange = new TextRange(
                        CodeDisplay.Document.ContentStart,
                        CodeDisplay.Document.ContentEnd
                    );

                    File.WriteAllText(saveFileDialog.FileName, textRange.Text);
                    StatusText.Text = $"Successfully saved to {saveFileDialog.FileName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}");
                    StatusText.Text = $"Error: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// Toggles dark mode for the application.
        /// </summary>
        private void ToggleDarkModeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleDarkMode();
        }

        private void ToggleDarkMode()
        {
            _darkMode = !_darkMode;

            Background = _darkMode ? Brushes.DarkSlateGray : Brushes.White;
            Foreground = _darkMode ? Brushes.White : Brushes.Black;

            CodeDisplay.Background = _darkMode ? Brushes.DimGray : Brushes.White;
            CodeDisplay.Foreground = _darkMode ? Brushes.White : Brushes.Black;

            StatusText.Foreground = _darkMode ? Brushes.LightGray : Brushes.Gray;

            foreach (var child in TopControlPanel.Children)
            {
                if (child is Button button)
                {
                    button.Background = _darkMode ? Brushes.SlateGray : SystemColors.ControlBrush;
                    button.Foreground = _darkMode ? Brushes.White : SystemColors.ControlTextBrush;
                }
                else if (child is CheckBox checkBox)
                {
                    checkBox.Foreground = _darkMode ? Brushes.White : SystemColors.ControlTextBrush;
                }
            }

            _syntaxHighlighter = new SyntaxHighlighter(_darkMode);

            if (CodeDisplay.Document != null)
            {
                var currentDoc = CodeDisplay.Document;
                var tempDoc = new FlowDocument();
                var range = new TextRange(currentDoc.ContentStart, currentDoc.ContentEnd);
                var rangeNew = new TextRange(tempDoc.ContentStart, tempDoc.ContentEnd);
                rangeNew.Text = range.Text;

                CodeDisplay.Document = tempDoc;
                CodeDisplay.Document = currentDoc;
            }
        }

        /// <summary>
        /// Refreshes the displayed folder contents.
        /// </summary>
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_lastSelectedFolderPath))
            {
                ProcessSelectedFolder(_lastSelectedFolderPath);
            }
            else
            {
                StatusText.Text = "No folder selected to refresh.";
            }
        }

        /// <summary>
        /// Clears the displayed code.
        /// </summary>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            CodeDisplay.Document.Blocks.Clear();
            StatusText.Text = "Code display cleared.";
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr LoadImage(IntPtr hinst, string lpszName, uint uType, int cxDesired, int cyDesired, uint fuLoad);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);
    }
}