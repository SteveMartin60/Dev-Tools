using HtmlAgilityPack;
using System;
using System.Linq;
using System.Text;

public class HtmlParser
{
    public static string ExtractHumanReadableText(string html)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(html);

        // Remove script and style elements
        var scriptNodes = htmlDoc.DocumentNode.SelectNodes("//script|//style|//noscript");
        if (scriptNodes != null)
        {
            foreach (var node in scriptNodes)
            {
                node.Remove();
            }
        }

        // Get all text nodes that are not empty and not just whitespace
        var textNodes = htmlDoc.DocumentNode.SelectNodes("//text()[normalize-space()]");
        if (textNodes == null)
        {
            return string.Empty;
        }

        // Filter and clean the text
        var sb = new StringBuilder();
        foreach (var node in textNodes)
        {
            string text = node.InnerText.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                sb.AppendLine(text);
            }
        }

        return sb.ToString();
    }
}

// Usage example:
// string html = "<div data-server-rendered..."; // Your HTML here
// string readableText = HtmlParser.ExtractHumanReadableText(html);
// Console.WriteLine(readableText);
