using CodeConsolidator.Models;
using CodeConsolidator.Services;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace CodeConsolidator.ViewModels
{
    public class MainViewModel
    {
        private readonly FileProcessor _fileProcessor;
        private readonly SyntaxHighlighter _syntaxHighlighter;

        public MainViewModel(bool darkMode)
        {
            _fileProcessor = new FileProcessor();
            _syntaxHighlighter = new SyntaxHighlighter(darkMode);
        }

        public void ProcessFolder(string folderPath, bool recursive, FlowDocument flowDocument)
        {
            var filteredFiles = _fileProcessor.GetFilteredFiles(folderPath, recursive);

            flowDocument.Blocks.Clear();

            // ------------------------------------------------------------------
            // Export information header (descriptive only)
            // ------------------------------------------------------------------

            flowDocument.Blocks.Add(new Paragraph(new Run("EXPORT INFO"))
            {
                FontWeight = FontWeights.Bold
            });

            flowDocument.Blocks.Add(new Paragraph(new Run(
                $"Generated (UTC): {DateTime.UtcNow:O}")));

            flowDocument.Blocks.Add(new Paragraph(new Run(
                $"Root folder: {folderPath.Replace('\\', '/')}")));

            flowDocument.Blocks.Add(new Paragraph(new Run(
                $"Files enumerated: {filteredFiles.Count}")));

            flowDocument.Blocks.Add(new Paragraph(new Run(
                "// ======================================================================"))
            {
                Foreground = Brushes.Gray
            });

            // ------------------------------------------------------------------
            // File contents
            // ------------------------------------------------------------------

            foreach (var filePath in filteredFiles)
            {
                AddFileHeader(flowDocument, filePath, folderPath);

                var fileContent = File.ReadAllText(filePath);

                if (filePath.EndsWith(".cs"))
                {
                    _syntaxHighlighter.HighlightCSharpCode(fileContent, flowDocument);
                }
                else if (filePath.EndsWith(".xaml") || filePath.EndsWith(".axaml"))
                {
                    _syntaxHighlighter.HighlightXamlCode(fileContent, flowDocument);
                }
                else if (filePath.EndsWith(".csproj"))
                {
                    _syntaxHighlighter.HighlightCsProjCode(fileContent, flowDocument);
                }
                else if (filePath.EndsWith(".sln"))
                {
                    _syntaxHighlighter.HighlightSlnCode(fileContent, flowDocument);
                }

                AddFileFooter(flowDocument, filePath, folderPath);
            }
        }

        private void AddFileHeader(FlowDocument flowDocument, string filePath, string folderPath)
        {
            string fileIcon = filePath.EndsWith(".cs") ? "◈" :
                      filePath.EndsWith(".xaml") ? "◇" :
                      filePath.EndsWith(".axaml") ? "◇" :
                      filePath.EndsWith(".csproj") ? "◉" : "⚙";

            var headerColor = filePath.EndsWith(".cs") ? Brushes.DodgerBlue :
                      filePath.EndsWith(".xaml") ? Brushes.Orange :
                      filePath.EndsWith(".axaml") ? Brushes.Orange :
                      filePath.EndsWith(".csproj") ? Brushes.MediumPurple :
                      Brushes.Green;

            var relativePath = Path.GetRelativePath(folderPath, filePath)
                           .Replace('\\', '/');

            flowDocument.Blocks.Add(new Paragraph(new Run("// ======================================================================"))
            {
                Foreground = Brushes.Gray
            });

            flowDocument.Blocks.Add(new Paragraph(new Run($"{fileIcon} Begin File: {relativePath}"))
            {
                Foreground = headerColor,
                FontWeight = FontWeights.Bold
            });

            flowDocument.Blocks.Add(new Paragraph(new Run("// -----------------------------------"))
            {
                Foreground = Brushes.Gray
            });

            flowDocument.Blocks.Add(new Paragraph());
        }

        private void AddFileFooter(FlowDocument flowDocument, string filePath, string folderPath)
        {
            string fileIcon = filePath.EndsWith(".cs") ? "◈" :
                      filePath.EndsWith(".xaml") ? "◇" :
                      filePath.EndsWith(".axaml") ? "◇" :
                      filePath.EndsWith(".csproj") ? "◉" : "⚙";

            var headerColor = filePath.EndsWith(".cs") ? Brushes.DodgerBlue :
                      filePath.EndsWith(".xaml") ? Brushes.Orange :
                      filePath.EndsWith(".axaml") ? Brushes.Orange :
                      filePath.EndsWith(".csproj") ? Brushes.MediumPurple :
                      Brushes.Green;

            var relativePath = Path.GetRelativePath(folderPath, filePath)
                           .Replace('\\', '/');

            flowDocument.Blocks.Add(new Paragraph(new Run("// -----------------------------------"))
            {
                Foreground = Brushes.Gray
            });

            flowDocument.Blocks.Add(new Paragraph(new Run($"{fileIcon} End File: {relativePath}"))
            {
                Foreground = headerColor,
                FontWeight = FontWeights.Bold
            });

            flowDocument.Blocks.Add(new Paragraph(new Run("// ======================================================================"))
            {
                Foreground = Brushes.Gray
            });

            flowDocument.Blocks.Add(new Paragraph());
        }
    }
}
