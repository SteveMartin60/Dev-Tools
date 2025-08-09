using HtmlAgilityPack;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace WebScraper
{
    public partial class MainWindow
    {
        private void LogElement(HtmlNode Element)
        {
            int Depth = Element.Depth;
            bool HasAttributes = Element.HasAttributes;
            bool HasChildNodes = Element.HasChildNodes;
            string NodeName = Element.Name;
            string OriginalName = Element.OriginalName;
            int ChildCount = Element.ChildNodes.Count;

            string LogString = string.Empty;

            LogString += "---------------------------------------------------------" + Environment.NewLine;
            LogString += $"Name: {NodeName}" + Environment.NewLine;
            LogString += "---------------------------" + Environment.NewLine;
            LogString += $"Depth        : {Depth}" + Environment.NewLine;
            LogString += $"HasAttributes: {HasAttributes}" + Environment.NewLine;
            LogString += $"HasChildNodes: {HasChildNodes}" + Environment.NewLine;
            LogString += $"ChildCount   : {ChildCount}" + Environment.NewLine;
            LogString += $"=========================================================" + Environment.NewLine;

            DoLog(LogString);
        }
        //.....................................................................

        //.....................................................................
        private void DoLog(string LogLine)
        {
            Debug.WriteLine(LogLine);

            if (LogLine.Contains("=====") || LogLine.Contains("-----") || LogLine.Contains("Processed") || LogLine.Length < 5)
            {
                return;
            }

            if (LogLine.Contains("/page/"))
            {
                Label_Status.Content = LogLine;

                MeshDoEvents(); 
            }
            else
            {
                LogLine = LogLine.Replace("\"", "");

                if (!Links.Contains(LogLine))
                {
                    Links.Add(LogLine);

                    MeshDoEvents();
                }

                if (!ListBox_Log.Items.Contains(LogLine))
                {
                    ListBox_Log.Items.Add(LogLine);

                    MeshDoEvents();

                    Debug.WriteLine(LogLine);

                    MeshDoEvents();
                }

            }

            MeshDoEvents();
        }
        //.....................................................................

        //.....................................................................
        private string SplitParseNode(HtmlNode Node)
        {
            List<string> ParsedNode = new List<string>();

            var TempText1 = Node.InnerText.Split("\n", StringSplitOptions.RemoveEmptyEntries);

            for (int j = 0; j < TempText1.Length; j++)
            {
                var Line = TempText1[j];

                if (Line.Length == 1)
                {
                    Line = string.Empty;
                }
                else if (Line.StartsWith("\n")) { Line = Line.Substring(1); }
                else if (Line.StartsWith("\t")) { Line = Line.Substring(1); }

                if (Line == "\t\t\t\t\t") { Line = string.Empty; }
                if (Line == "\t\t\t\t"  ) { Line = string.Empty; }
                if (Line == "\t\t\t"    ) { Line = string.Empty; }
                if (Line == "\t\t"      ) { Line = string.Empty; }
                if (Line == "\t"        ) { Line = string.Empty; }

                if (Line.Contains("\t\t"))
                {
                    while (Line.Contains("\t\t"))
                    {
                        Line = Line.Replace("\t\t", "\t");
                    }
                }

                if (Line.Length == 1)
                {
                    Line = string.Empty;
                }
                else if (Line.StartsWith("\n")) { Line = Line.Substring(1); }
                else if (Line.StartsWith("\t")) { Line = Line.Substring(1); }

                if (Line != string.Empty)
                {
                    ParsedNode.Add(Line);
                }
            }

            string Result = string.Concat(ParsedNode);

            return Result;
        }
        //.....................................................................

        //.....................................................................
        public static string FormatJson(string json, string indent = "  ")
        {
            var indentation = 0;
            var quoteCount = 0;
            var escapeCount = 0;

            var result =
                from ch in json ?? string.Empty
                let escaped = (ch == '\\' ? escapeCount++ : escapeCount > 0 ? escapeCount-- : escapeCount) > 0
                let quotes = ch == '"' && !escaped ? quoteCount++ : quoteCount
                let unquoted = quotes % 2 == 0
                let colon = ch == ':' && unquoted ? ": " : null
                let nospace = char.IsWhiteSpace(ch) && unquoted ? string.Empty : null
                let lineBreak = ch == ',' && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indent, indentation)) : null
                let openChar = (ch == '{' || ch == '[') && unquoted ? ch + Environment.NewLine + string.Concat(Enumerable.Repeat(indent, ++indentation)) : ch.ToString()
                let closeChar = (ch == '}' || ch == ']') && unquoted ? Environment.NewLine + string.Concat(Enumerable.Repeat(indent, --indentation)) + ch : ch.ToString()
                select colon ?? nospace ?? lineBreak ?? (
                    openChar.Length > 1 ? openChar : closeChar
                );

            return string.Concat(result);
        }
        //.....................................................................

        //.....................................................................
        public static void MeshDoEvents()
        {
            var T = Application.Current;

            if (Application.Current == null)
            {
                return;
            }

            try
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
            }
            catch (Exception exception)
            {

            }
        }
        //.....................................................................

        //.....................................................................
        private static ScrollViewer FindScrollViewer(DependencyObject root)
        {
            var queue = new Queue<DependencyObject>(new[] { root });

            do
            {
                var item = queue.Dequeue();

                if (item is ScrollViewer)
                    return (ScrollViewer)item;

                for (var i = 0; i < VisualTreeHelper.GetChildrenCount(item); i++)
                    queue.Enqueue(VisualTreeHelper.GetChild(item, i));
            } while (queue.Count > 0);

            return null;
        }
        //.....................................................................

        //.....................................................................
        private void ListBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox)sender;

            var scrollViewer = FindScrollViewer(listBox);

            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += (o, args) =>
                {
                    if (args.ExtentHeightChange > 0)
                        scrollViewer.ScrollToBottom();
                };
            }
        }
        //.....................................................................

        //.....................................................................
        private void ListBox_Log_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxLinks.SelectedItem is not null)
            {
                FileInfo JsonFile = ListBoxLinks.SelectedItem as FileInfo;
            }
        }
        //.....................................................................

        //.....................................................................
        public static string DumpHtmlNode(HtmlNode node)
        {
            var sb = new StringBuilder();
            string Result = string.Empty;

            sb.AppendLine("=== HTML NODE FULL DUMP ===");
            sb.AppendLine($"Name: {node.Name}");
            sb.AppendLine($"NodeType: {node.NodeType}");
            sb.AppendLine($"Depth: {node.Depth}");
            sb.AppendLine($"Id: {node.Id}");
            sb.AppendLine($"Line: {node.Line}");
            sb.AppendLine();

            // Attributes
            sb.AppendLine("=== ATTRIBUTES ===");
            if (node.Attributes != null && node.Attributes.Count > 0)
            {
                foreach (var attr in node.Attributes)
                {
                    sb.AppendLine($"{attr.Name} = {attr.Value} (Line: {attr.Line})");
                }
            }
            else
            {
                sb.AppendLine("No attributes");
            }
            sb.AppendLine();

            // Parent Node
            sb.AppendLine("=== PARENT NODE ===");
            if (node.ParentNode != null)
            {
                sb.AppendLine($"Name: {node.ParentNode.Name}");
                sb.AppendLine($"NodeType: {node.ParentNode.NodeType}");
            }
            else
            {
                sb.AppendLine("No parent node (this is likely the document node)");
            }
            sb.AppendLine();

            // Owner Document
            sb.AppendLine("=== OWNER DOCUMENT ===");
            if (node.OwnerDocument != null)
            {
                sb.AppendLine($"Document has {node.OwnerDocument.DocumentNode.ChildNodes.Count} top-level nodes");
                sb.AppendLine($"Parse errors: {node.OwnerDocument.ParseErrors?.Count() ?? 0}");
            }
            else
            {
                sb.AppendLine("No owner document");
            }
            sb.AppendLine();

            // Child Nodes
            sb.AppendLine("=== CHILD NODES ===");
            if (node.ChildNodes != null && node.ChildNodes.Count > 0)
            {
                sb.AppendLine($"Total child nodes: {node.ChildNodes.Count}");
                sb.AppendLine("First 5 child nodes (or all if less than 5):");

                int maxToShow = Math.Min(5, node.ChildNodes.Count);
                for (int i = 0; i < maxToShow; i++)
                {
                    var child = node.ChildNodes[i];
                    sb.AppendLine($"[{i}] {child.Name} ({child.NodeType}) Line: {child.Line}");
                }

                if (node.ChildNodes.Count > 5)
                {
                    sb.AppendLine($"[... {node.ChildNodes.Count - 5} more child nodes not shown ...]");
                }
            }
            else
            {
                sb.AppendLine("No child nodes");
            }
            sb.AppendLine();

            // HTML Content
            sb.AppendLine("=== INNER HTML ===");
            sb.AppendLine(node.InnerHtml);
            sb.AppendLine();

            sb.AppendLine("=== OUTER HTML ===");
            sb.AppendLine(node.OuterHtml);

            Result = sb.ToString();

            return Result;
        }
        //.....................................................................

        //.....................................................................
    }
}


