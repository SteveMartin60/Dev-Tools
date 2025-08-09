using Microsoft.Win32;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace IconExtractor
{
    public partial class MainWindow : Window
    {
        public class IconInfo
        {
            public int          Index   { get; set; }
            public BitmapSource Preview { get; set; }
            public string       Size    { get; set; }
            public string       Format  { get; set; }
            public Icon         Icon    { get; set; }
        }

        public string DllPath { get; set; }
        public string DllName { get; set; }
        private List<IconInfo> icons = new List<IconInfo>();

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]        static extern uint ExtractIconEx(string szFileName, int nIconIndex,            IntPtr[] phiconLarge, IntPtr[] phiconSmall, uint nIcons);
        [DllImport("user32.dll", SetLastError = true)]        static extern bool DestroyIcon(IntPtr hIcon);

        public MainWindow()
        {
            InitializeComponent();

            // Set window title
            this.Title = "IconExtractor";

            // Set default output folder
            txtOutputFolder.Text = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "ExtractedIcons");

            // Load common DLLs with icons
            LoadCommonDlls();

            // Set up event handler for DLL selection
            ListBox_Dlls.SelectionChanged += ListBox_Dlls_SelectionChanged;
        }

        private void LoadCommonDlls()
        {
            var system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);

            var commonDlls = new List<string>    
            {
                // Shell and UI related
                Path.Combine(system32, "shell32.dll"),
                Path.Combine(system32, "imageres.dll"),
                Path.Combine(system32, "pifmgr.dll"),
                Path.Combine(system32, "compstui.dll"),
                Path.Combine(system32, "moricons.dll"),
                Path.Combine(system32, "mmres.dll"),
                Path.Combine(system32, "netshell.dll"),
                Path.Combine(system32, "setupapi.dll"),
                Path.Combine(system32, "wmploc.dll"),
                Path.Combine(system32, "ddores.dll"),
        
                // Network related
                Path.Combine(system32, "netcenter.dll"),
                Path.Combine(system32, "netcfgx.dll"),
                Path.Combine(system32, "netman.dll"),
        
                // Hardware and devices
                Path.Combine(system32, "devicecenter.dll"),
                Path.Combine(system32, "devmgr.dll"),
                Path.Combine(system32, "hdwwiz.cpl"),
                Path.Combine(system32, "stobject.dll"),
        
                // System utilities
                Path.Combine(system32, "appwiz.cpl"),
                Path.Combine(system32, "bthprops.cpl"),
                Path.Combine(system32, "desk.cpl"),
                Path.Combine(system32, "firewallcontrolpanel.dll"),
                Path.Combine(system32, "main.cpl"),
                Path.Combine(system32, "mmsys.cpl"),
                Path.Combine(system32, "ncpa.cpl"),
                Path.Combine(system32, "powercfg.cpl"),
                Path.Combine(system32, "sysdm.cpl"),
                Path.Combine(system32, "telephon.cpl"),
                Path.Combine(system32, "timedate.cpl"),
        
                // Windows components
                Path.Combine(system32, "comdlg32.dll"),
                Path.Combine(system32, "explorerframe.dll"),
                Path.Combine(system32, "ieframe.dll"),
                Path.Combine(system32, "mshtml.dll"),
                Path.Combine(system32, "msi.dll"),
                Path.Combine(system32, "ole32.dll"),
                Path.Combine(system32, "olepro32.dll"),
                Path.Combine(system32, "sensorsutilsv2.dll"),
                Path.Combine(system32, "shdocvw.dll"),
                Path.Combine(system32, "urlmon.dll"),
                Path.Combine(system32, "zipfldr.dll"),
        
                // Multimedia
                Path.Combine(system32, "audiodev.dll"),
                Path.Combine(system32, "dsound.dll"),
                Path.Combine(system32, "msacm32.dll"),
                Path.Combine(system32, "msvidc32.dll"),
                Path.Combine(system32, "wiashext.dll"),
        
                // Security
                Path.Combine(system32, "authui.dll"),
                Path.Combine(system32, "credui.dll"),
                Path.Combine(system32, "cryptui.dll"),
                Path.Combine(system32, "wucltux.dll"),
        
                // Windows 10/11 specific
                Path.Combine(system32, "actioncenter.dll"),
                Path.Combine(system32, "bcastdvr.exe"),
                Path.Combine(system32, "cloudstoragewizard.exe"),
                Path.Combine(system32, "filemanager.dll"),
                Path.Combine(system32, "gameux.dll"),
                Path.Combine(system32, "msdt.exe"),
                Path.Combine(system32, "twinui.dll"),
                Path.Combine(system32, "windows.ui.xaml.dll"),
        
                // Control Panel items
                Path.Combine(system32, "accessibilitycpl.dll"),
                Path.Combine(system32, "inetcpl.cpl"),
                Path.Combine(system32, "intl.cpl"),
                Path.Combine(system32, "irprops.cpl"),
                Path.Combine(system32, "joy.cpl"),
                Path.Combine(system32, "odbccp32.cpl"),
                Path.Combine(system32, "wscui.cpl"),
        
                // System tools
                Path.Combine(system32, "cleanmgr.exe"),
                Path.Combine(system32, "dfrgui.exe"),
                Path.Combine(system32, "diskmgmt.msc"),
                Path.Combine(system32, "eventvwr.exe"),
                Path.Combine(system32, "perfmon.exe"),
                Path.Combine(system32, "taskmgr.exe"),
                Path.Combine(system32, "verifier.exe")
            };

            // Add only DLLs that exist
            foreach (var dll in commonDlls)
            {
                if (File.Exists(dll))
                {
                    ListBox_Dlls.Items.Add(dll);
                }
            }
        }
        
        private void LoadIcons(string dllPath)
        {
            lvIcons.ItemsSource = null;
            icons.Clear();

            if (!File.Exists(dllPath))
            {
                MessageBox.Show("The specified DLL file does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                // First, try to get the icon count
                uint iconCount = ExtractIconEx(dllPath, -1, null, null, 0);

                for (int i = 0; i < iconCount; i++)
                {
                    IntPtr[] largeIcons = new IntPtr[1];
                    uint extracted = ExtractIconEx(dllPath, i, largeIcons, null, 1);

                    if (extracted > 0 && largeIcons[0] != IntPtr.Zero)
                    {
                        using (Icon icon = System.Drawing.Icon.FromHandle(largeIcons[0]))
                        {
                            var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(
                                icon.Handle,
                                Int32Rect.Empty,
                                BitmapSizeOptions.FromEmptyOptions());

                            icons.Add(new IconInfo
                            {
                                Index = i,
                                Preview = bitmapSource,
                                Size = $"{icon.Width}x{icon.Height}",
                                Format = icon.Width == icon.Height ? "Square" : "Rectangle",
                                Icon = (Icon)icon.Clone()
                            });
                        }

                        DestroyIcon(largeIcons[0]);
                    }
                }

                lvIcons.ItemsSource = icons;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading icons: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ListBox_Dlls_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ListBox_Dlls.SelectedItem != null)
            {
                DllPath = ListBox_Dlls.SelectedItem.ToString();

                DllName = Path.GetFileNameWithoutExtension(DllPath);

                LoadIcons(DllPath);
            }
        }

        private void BtnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "DLL files (*.dll)|*.dll|Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.System)
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedDll = openFileDialog.FileName;
                if (!ListBox_Dlls.Items.Contains(selectedDll))
                {
                    ListBox_Dlls.Items.Add(selectedDll);
                }
                ListBox_Dlls.SelectedItem = selectedDll;
            }
        }

        private void BtnBrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            // Using OpenFileDialog in folder selection mode as a workaround
            var openFolderDialog = new OpenFileDialog
            {
                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Folder Selection",
                Title = "Select output folder for extracted icons"
            };

            if (openFolderDialog.ShowDialog() == true)
            {
                string selectedPath = Path.GetDirectoryName(openFolderDialog.FileName);
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    txtOutputFolder.Text = selectedPath;
                }
            }
        }

        private void BtnExtractAll_Click(object sender, RoutedEventArgs e)
        {
            ExtractIcons(false);
        }

        private void BtnExtractSelected_Click(object sender, RoutedEventArgs e)
        {
            ExtractIcons(true);
        }

        private void ExtractIcons(bool onlySelected)
        {
            string outputFolder = txtOutputFolder.Text;

            try
            {
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                int count = 0;
                var itemsToExtract = onlySelected ? lvIcons.SelectedItems : lvIcons.Items;

                // Create subdirectories for extracted icons
                string pngFolder = Path.Combine(outputFolder, DllName, "png");
                string icoFolder = Path.Combine(outputFolder, DllName, "ico");
                Directory.CreateDirectory(pngFolder);
                Directory.CreateDirectory(icoFolder);

                // First extract all icons as PNG
                foreach (IconInfo item in itemsToExtract)
                {
                    // Save as PNG (32-bit with alpha)
                    string pngFileName = Path.Combine(pngFolder, $"icon_{item.Index}.png");
                    string icoFileName = Path.Combine(icoFolder, $"icon_{item.Index}.ico");

                    using (Bitmap bitmap = new Bitmap(item.Icon.Width, item.Icon.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
                    {
                        using (Graphics g = Graphics.FromImage(bitmap))
                        {
                            g.Clear(System.Drawing.Color.Transparent);
                            g.DrawIcon(item.Icon, 0, 0);
                        }

                        bitmap.Save(pngFileName, ImageFormat.Png);

                        if (File.Exists(pngFileName))
                        { 
                            using (var png = (Bitmap)Image.FromFile(pngFileName))
                            {
                                PngToIconConverter.ConvertPngToIco(png, icoFileName);

                                Console.WriteLine($"Converted {png.Width}x{png.Height} PNG to ICO");
                            }
                        }
                    }

                    count++;
                }

                Debug.WriteLine($"Successfully extracted {count} icons (as .png and .ico) to:\nPNG: {pngFolder}\nICO: {icoFolder}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error extracting icons: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnExportHtml_Click(object sender, RoutedEventArgs e)
        {
            if (ListBox_Dlls.SelectedItem == null)
            {
                MessageBox.Show("Please select a DLL file first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string outputFolder = txtOutputFolder.Text;

            try
            {
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                IconsToComposite.ExtractIconsToCompositeImage(DllPath, outputFolder);
                MessageBox.Show($"Composite image and individual icons (as both .ico and .png) have been created in:\n{outputFolder}", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating output: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    class PngToIconConverter
    {
        public static bool ConvertPngToIco(Bitmap sourceBitmap, string icoPath)
        {
            // Get the original dimensions from the PNG
            int width = sourceBitmap.Width;
            int height = sourceBitmap.Height;

            try
            {
                using (var memoryStream = new MemoryStream())
                {
                    using (var iconWriter = new BinaryWriter(memoryStream))
                    {
                        // ICO header (6 bytes)
                        iconWriter.Write((short)0);  // Reserved
                        iconWriter.Write((short)1);  // Type (1 for ICO)
                        iconWriter.Write((short)1);  // Number of images (just this one)

                        // Image entry (16 bytes)
                        iconWriter.Write((byte )width ); // Width
                        iconWriter.Write((byte )height); // Height
                        iconWriter.Write((byte )0     ); // Color palette (0 for no palette)
                        iconWriter.Write((byte )0     ); // Reserved
                        iconWriter.Write((short)1     ); // Color planes
                        iconWriter.Write((short)32    ); // Bits per pixel (32-bit ARGB)

                        // Convert the bitmap to PNG format for the ICO
                        byte[] imageData;
                        using (var bitmapStream = new MemoryStream())
                        {
                            sourceBitmap.Save(bitmapStream, ImageFormat.Png);
                            imageData = bitmapStream.ToArray();
                        }

                        iconWriter.Write(imageData.Length);  // Size of image data
                        iconWriter.Write(22);                // Offset to image data (6 + 16)
                        iconWriter.Write(imageData);         // The actual image data
                    }

                    File.WriteAllBytes(icoPath, memoryStream.ToArray());
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error converting to ICO: {ex.Message}");
                return false;
            }
        }
    }
}
