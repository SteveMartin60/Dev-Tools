using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace WpfCodeGenerator
{
    public partial class MainWindow : Window
    {
        private const string DefaultFolderPath = @"D:\dev-test\pcb-imaging-automation-review\";

        public MainWindow()
        {
            InitializeComponent();

            // Set the default folder path in the OutputFolderTextBox
            OutputFolderTextBox.Text = DefaultFolderPath;
        }

        private void BrowseOutputFolder_Click(object sender, RoutedEventArgs e)
        {
            // Use OpenFileDialog configured as a folder picker
            var folderDialog = new OpenFileDialog
            {
                Title = "Select Output Folder",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Folder Selection.", // Dummy file name to enable folder selection
                Filter = "Folder|*.thisfolderdoesnotexist", // Dummy filter
                InitialDirectory = string.IsNullOrEmpty(OutputFolderTextBox.Text)
                    ? DefaultFolderPath
                    : OutputFolderTextBox.Text // Open dialog at the folder specified in the textbox
            };

            if (folderDialog.ShowDialog() == true)
            {
                // Extract the folder path from the dialog result
                string folderPath = Path.GetDirectoryName(folderDialog.FileName);
                OutputFolderTextBox.Text = folderPath;
            }
        }

        private void GenerateProject_Click(object sender, RoutedEventArgs e)
        {
            string projectName     = ProjectNameTextBox    .Text.Trim();
            string mainWindowTitle = MainWindowTitleTextBox.Text.Trim();
            string mainWindowName  = MainWindowNameTextBox .Text.Trim();
            string rootNamespace   = RootNamespaceTextBox  .Text.Trim();
            string outputFolder    = OutputFolderTextBox   .Text.Trim();

            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(mainWindowName) ||
                string.IsNullOrEmpty(rootNamespace) || string.IsNullOrEmpty(outputFolder))
            {
                MessageBox.Show("Please fill in all fields.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                GenerateWpfProject(projectName, mainWindowName, mainWindowTitle, rootNamespace, outputFolder);
                MessageBox.Show("Project generated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating project: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateWpfProject(string projectName, string mainWindowName, string mainWindowTitle, string rootNamespace, string outputFolder)
        {
            string projectPath = Path.Combine(outputFolder, projectName);
            Directory.CreateDirectory(projectPath);

            // Create .csproj file
            string csprojContent = $@"
<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net9.0-windows</TargetFramework>
    <Nullable>enable</Nullable>
    <UseWPF>true</UseWPF>
    <RootNamespace>{rootNamespace}</RootNamespace>
  </PropertyGroup>
</Project>";

            File.WriteAllText(Path.Combine(projectPath, $"{projectName}.csproj"), csprojContent);

            // Create App.xaml
            string appXamlContent = $@"
<Application x:Class=""{rootNamespace}.App""
             xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
             xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
             xmlns:local=""clr-namespace:{rootNamespace}""
             StartupUri=""{mainWindowName}.xaml"">
    <Application.Resources>
    </Application.Resources>
</Application>";

            File.WriteAllText(Path.Combine(projectPath, "App.xaml"), appXamlContent);

            // Create App.xaml.cs
            string appXamlCsContent = $@"
using System.Windows;

namespace {rootNamespace}
{{
    public partial class App : Application
    {{
    }}
}}";

            File.WriteAllText(Path.Combine(projectPath, "App.xaml.cs"), appXamlCsContent);

            // Create MainWindow.xaml
            string mainWindowXamlContent = $@"
<Window x:Class=""{rootNamespace}.{mainWindowName}""
        xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
        xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
        xmlns:local=""clr-namespace:{rootNamespace}""
        Title=""{mainWindowTitle}"" Height=""450"" Width=""800"">
    <Grid>
    </Grid>
</Window>";

            File.WriteAllText(Path.Combine(projectPath, $"{mainWindowName}.xaml"), mainWindowXamlContent);

            // Create MainWindow.xaml.cs
            string mainWindowXamlCsContent = $@"
using System.Windows;

namespace {rootNamespace}
{{
    public partial class {mainWindowName} : Window
    {{
        public {mainWindowName}()
        {{
            InitializeComponent();
        }}
    }}
}}";

            File.WriteAllText(Path.Combine(projectPath, $"{mainWindowName}.xaml.cs"), mainWindowXamlCsContent);
        }
    }
}