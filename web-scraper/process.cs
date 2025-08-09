using HtmlAgilityPack;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Windows.Controls;

namespace WebScraper
{
    public partial class MainWindow
    {
        List<HtmlNode>? NodesHrefFiltered;
        //.....................................................................

        //.....................................................................
        private async Task ProcessYouDaoPage(string WebPageAddress, string ReferrenceString)
        {
            const int LineLength = 190;

            string CommentLineBefore = Environment.NewLine + new string('=', LineLength);
            string CommentLineAfter = new string('-', LineLength) + Environment.NewLine;
            
            HtmlWeb TranslationWeb = new HtmlWeb();
            
            TranslationWeb.OverrideEncoding = Encoding.UTF8;

            HttpClient TranslationHttpClient = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });

            HttpResponseMessage TranslationResponse = await TranslationHttpClient.GetAsync(WebPageAddress);

            Stream TranslationStream = await TranslationResponse.Content.ReadAsStreamAsync();

            HtmlDocument TranslationHtmlDoc = TranslationWeb.Load(WebPageAddress);

            string xpath = $"//*[contains(text(), '{ReferrenceString}')]";

            HtmlNode TranslationNode = TranslationHtmlDoc.DocumentNode.SelectSingleNode(xpath);

            HtmlAttributeCollection TranslationAttributes = TranslationNode.Attributes;

            string TranslatedString = TranslationNode.InnerText;

            if (TranslationNode != null)
            {
                DoLog(DumpHtmlNode(TranslationNode));

                DoLog(CommentLineBefore);
                DoLog("Found Translation:");
                DoLog(TranslationNode.OuterHtml);
                DoLog("Text content:");
                DoLog(TranslationNode.InnerText.Trim());
                DoLog(CommentLineAfter);
            }
            else
            {
                DoLog("No element contains the specified text.");
            }

            DoLog($"Processed: {WebPageAddress}");
        }
        //.....................................................................

        //.....................................................................

    }
}


