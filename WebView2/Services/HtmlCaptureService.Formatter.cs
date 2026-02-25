using HtmlAgilityPack;
using System.IO;
using System.Text;

namespace WebView2Browser.Services
{
    public partial class HtmlCaptureService
    {
        public string FormatHtmlWithBetterIndentation(string rawHtml)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(rawHtml);
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                foreach (var node in doc.DocumentNode.ChildNodes)
                    WriteNodeWithImprovedFormatting(node, writer, 0);
            }
            return sb.ToString();
        }
        private void WriteNodeWithImprovedFormatting(HtmlNode node, TextWriter writer, int indentLevel)
        {
            string indent = new string(' ', indentLevel * 4);
            string childIndent = new string(' ', (indentLevel + 1) * 4);
            switch (node.NodeType)
            {
                case HtmlNodeType.Element:
                    if (IsInlineElement(node.Name) || node.HasClass("prettier-ignore-start") || node.HasClass("prettier-ignore-end"))
                    { writer.Write(node.OuterHtml); return; }
                    writer.Write($"{indent}<{node.Name}");
                    if (node.HasAttributes)
                    {
                        foreach (var attr in node.Attributes)
                            writer.Write($" {attr.Name}=\"{attr.Value}\"");
                    }
                    if (node.ChildNodes.Count == 0 && HtmlNode.IsEmptyElement(node.Name))
                        writer.WriteLine(" />");
                    else
                    {
                        writer.WriteLine(">");
                        if (node.ChildNodes.Count == 1 && node.FirstChild.NodeType == HtmlNodeType.Text)
                        {
                            writer.Write(childIndent);
                            WriteNodeWithImprovedFormatting(node.FirstChild, writer, 0);
                            writer.WriteLine();
                        }
                        else
                        {
                            foreach (var child in node.ChildNodes)
                                WriteNodeWithImprovedFormatting(child, writer, indentLevel + 1);
                        }
                        writer.WriteLine($"{indent}</{node.Name}>");
                    }
                    break;
                case HtmlNodeType.Text:
                    string text = node.InnerText.Trim();
                    if (!string.IsNullOrEmpty(text)) writer.Write(text);
                    break;
                case HtmlNodeType.Comment:
                    writer.WriteLine($"{indent}<!--{node.InnerHtml}-->");
                    break;
                default: writer.Write(node.OuterHtml); break;
            }
        }
        private bool IsInlineElement(string tagName)
        {
            return tagName switch
            {
                "a" or "span" or "strong" or "em" or "b" or "i" or "img" or "svg" or "use" => true,
                _ => false
            };
        }
    }
}
