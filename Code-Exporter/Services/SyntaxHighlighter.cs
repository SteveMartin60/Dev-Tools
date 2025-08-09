using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CodeConsolidator.Services
{
    public class SyntaxHighlighter
    {
        private readonly bool _darkMode;

        public SyntaxHighlighter(bool darkMode)
        {
            _darkMode = darkMode;
        }

        public void HighlightCSharpCode(string code, FlowDocument flowDocument)
        {
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var keywords = new[] { "using", "namespace", "class", "void", "return", "if", "else", "for", "foreach", "while", "do", "switch", "case", "break", "continue", "var", "new", "this", "base", "true", "false", "null" };
            int lineNumber = 1;


            for (int i = 0; i < lines.Length; i++)
            //foreach (var line in lines)
            {
                var line = lines[i];

                var paragraph = CreateParagraph(lineNumber++, line);

                foreach (var keyword in keywords)
                {
                    int index = 0;
                    while (index >= 0 && index < line.Length)
                    {
                        index = line.IndexOf(keyword, index, StringComparison.Ordinal);
                        if (index < 0) break;

                        bool isWholeWord = (index == 0 || !char.IsLetterOrDigit(line[index - 1])) &&
                                           (index + keyword.Length >= line.Length ||
                                            !char.IsLetterOrDigit(line[index + keyword.Length]));

                        if (isWholeWord)
                        {
                            AddTextBeforeKeyword(paragraph, line.Substring(0, index));
                            AddHighlightedKeyword(paragraph, keyword);
                            line = line.Substring(index + keyword.Length);
                            index = 0;
                        }
                        else
                        {
                            index += keyword.Length;
                        }
                    }
                }

                AddRemainingText(paragraph, line);
                flowDocument.Blocks.Add(paragraph);
            }
        }

        public void HighlightXamlCode(string code, FlowDocument flowDocument)
        {
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int lineNumber = 1;

            foreach (var line in lines)
            {
                var paragraph = CreateParagraph(lineNumber++, line);
                var processedLine = line;

                int tagStart = 0;
                while ((tagStart = processedLine.IndexOf('<')) >= 0)
                {
                    AddTextBeforeTag(paragraph, processedLine.Substring(0, tagStart));
                    processedLine = processedLine.Substring(tagStart);
                    tagStart = 0;

                    int tagEnd = processedLine.IndexOf('>');
                    if (tagEnd < 0)
                    {
                        AddUnclosedTag(paragraph, processedLine);
                        break;
                    }

                    var tag = processedLine.Substring(0, tagEnd + 1);
                    AddHighlightedTag(paragraph, tag);
                    processedLine = processedLine.Substring(tagEnd + 1);
                }

                AddRemainingText(paragraph, processedLine);
                flowDocument.Blocks.Add(paragraph);
            }
        }

        // Add this method to the SyntaxHighlighter class in Services/SyntaxHighlighter.cs
        public void HighlightCsProjCode(string code, FlowDocument flowDocument)
        {
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int lineNumber = 1;

            foreach (var line in lines)
            {
                var paragraph = CreateParagraph(lineNumber++, line);
                var processedLine = line;

                // Highlight XML tags
                int tagStart = 0;
                while ((tagStart = processedLine.IndexOf('<')) >= 0)
                {
                    AddTextBeforeTag(paragraph, processedLine.Substring(0, tagStart));
                    processedLine = processedLine.Substring(tagStart);
                    tagStart = 0;

                    int tagEnd = processedLine.IndexOf('>');
                    if (tagEnd < 0)
                    {
                        AddUnclosedTag(paragraph, processedLine);
                        break;
                    }

                    var tag = processedLine.Substring(0, tagEnd + 1);

                    // Special highlighting for important csproj elements
                    if (tag.StartsWith("<Project") || tag.StartsWith("</Project") ||
                        tag.StartsWith("<PropertyGroup") || tag.StartsWith("</PropertyGroup") ||
                        tag.StartsWith("<ItemGroup") || tag.StartsWith("</ItemGroup"))
                    {
                        paragraph.Inlines.Add(new Run(tag)
                        {
                            Foreground = _darkMode ? Brushes.LightGreen : Brushes.DarkGreen,
                            FontWeight = FontWeights.Bold
                        });
                    }
                    else if (tag.StartsWith("<PackageReference") || tag.StartsWith("<ProjectReference") ||
                             tag.StartsWith("<Reference") || tag.StartsWith("<Compile") ||
                             tag.StartsWith("<None") || tag.StartsWith("<Content"))
                    {
                        paragraph.Inlines.Add(new Run(tag)
                        {
                            Foreground = _darkMode ? Brushes.LightBlue : Brushes.Blue
                        });
                    }
                    else
                    {
                        AddHighlightedTag(paragraph, tag);
                    }

                    processedLine = processedLine.Substring(tagEnd + 1);
                }

                AddRemainingText(paragraph, processedLine);
                flowDocument.Blocks.Add(paragraph);
            }
        }

        public void HighlightSlnCode(string code, FlowDocument flowDocument)
        {
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            int lineNumber = 1;

            foreach (var line in lines)
            {
                var paragraph = CreateParagraph(lineNumber++, line);

                if (line.Trim().StartsWith("Project("))
                {
                    // Highlight project references
                    AddHighlightedKeyword(paragraph, "Project");
                    AddTextBeforeKeyword(paragraph, line.Substring(0, line.IndexOf("Project")));
                    AddRemainingText(paragraph, line.Substring(line.IndexOf("Project") + "Project".Length));
                }
                else if (line.Trim().StartsWith("GlobalSection("))
                {
                    // Highlight global sections
                    AddHighlightedKeyword(paragraph, "GlobalSection");
                    AddTextBeforeKeyword(paragraph, line.Substring(0, line.IndexOf("GlobalSection")));
                    AddRemainingText(paragraph, line.Substring(line.IndexOf("GlobalSection") + "GlobalSection".Length));
                }
                else
                {
                    // Default text
                    AddRemainingText(paragraph, line);
                }

                flowDocument.Blocks.Add(paragraph);
            }
        }

        private Paragraph CreateParagraph(int lineNumber, string line)
        {
            var paragraph = new Paragraph();
            paragraph.Inlines.Add(new Run($"{lineNumber.ToString().PadLeft(4)} | ")
            {
                Foreground = Brushes.Gray,
                FontFamily = new FontFamily("Consolas")
            });
            return paragraph;
        }

        private void AddTextBeforeKeyword(Paragraph paragraph, string text)
        {
            paragraph.Inlines.Add(new Run(text));
        }

        private void AddHighlightedKeyword(Paragraph paragraph, string keyword)
        {
            paragraph.Inlines.Add(new Run(keyword)
            {
                Foreground = _darkMode ? Brushes.LightBlue : Brushes.Blue,
                FontWeight = FontWeights.Bold
            });
        }

        private void AddHighlightedTag(Paragraph paragraph, string tag)
        {
            paragraph.Inlines.Add(new Run(tag)
            {
                Foreground = _darkMode ? Brushes.LightCoral : Brushes.DarkRed
            });
        }

        private void AddUnclosedTag(Paragraph paragraph, string tag)
        {
            paragraph.Inlines.Add(new Run(tag)
            {
                Foreground = _darkMode ? Brushes.LightCoral : Brushes.DarkRed
            });
        }

        private void AddTextBeforeTag(Paragraph paragraph, string text)
        {
            paragraph.Inlines.Add(new Run(text));
        }

        private void AddRemainingText(Paragraph paragraph, string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                paragraph.Inlines.Add(new Run(text));
            }
        }
    }
}
