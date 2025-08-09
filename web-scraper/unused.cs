using HtmlAgilityPack;
using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace WebScraper
{
    public partial class MainWindow
    {
        private async void ProcessWebPageOld(string WebPageAddress)
        {
            if (!ValidateUrl(WebPageAddress))
            {
                return;
            }

            List<Link> Links = new List<Link>();

            string CommentLineBefore = $"{Environment.NewLine}==============================================================================================================================================================================================";
            string CommentLineAfter = $"----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------{Environment.NewLine}";
            string CommentLineShort = $"-----------------------------";

            string ParsedNode;

            //................................................................
            #region Begin Region Get Nodes
            //................................................................
            HtmlWeb web = new HtmlWeb();

            DoLog("Create Web Client");

            HttpClient httpClient = new HttpClient(new HttpClientHandler() { UseDefaultCredentials = true });

            response = await httpClient.GetAsync(WebPageAddress);

            DoLog("Get Stream Response");
            Stream StreamResponse = await response.Content.ReadAsStreamAsync();

            DoLog("Load Web Page");
            HtmlDocument HtmlDoc = web.Load(WebPageAddress);

            DoLog("Load Stream Response");
            HtmlDoc.Load(StreamResponse);

            var Descendants = HtmlDoc.DocumentNode.Descendants();

            List<HtmlNode> NodesAll = Descendants.ToList();
            DoLog($"Got All Nodes: {NodesAll.Count}");
            //................................................................
            #endregion End Region Get Nodes
            //................................................................

            int AllNodesCount = NodesAll.Count();

            var PI = NodesAll.Where(Node => Node.OuterHtml.ToLower().Contains("piss"));
            var PR = NodesAll.Where(Node => Node.OuterHtml.ToLower().Contains("#private") && Node.InnerHtml != "").ToList();
            var PU = NodesAll.Where(Node => !Node.OuterHtml.ToLower().Contains("#private") && Node.InnerHtml != "").ToList();

            List<HtmlNode> NodesHref = HtmlDoc.DocumentNode.SelectNodes("//a[@href]")
                .Where(Node =>
                     Node.Attributes.Contains("Title") &&
                    !Node.OuterHtml.Contains("bg_private_video.gif") &&
                     Node.OuterHtml.Contains("/videos/")
                )
                .ToList();

            for (int i = 0; i < NodesHref.Count; i++)
            {
                var Node = NodesHref[i];

                var Title = Node.Attributes.ToList().Where(A => A.Name == "title").ToList()[0].Value;
                var Link = Node.Attributes.ToList().Where(A => A.Name == "href").ToList()[0].Value;

                Links.Add(new Link(Title, Link));

                DoLog($"{Title.PadRight(45)} {Link}");

                List<string> A = Node.OuterHtml.Split("href").ToList();

                List<string> B = Node.InnerHtml.Split("href").ToList();
            }

            var T = NodesAll.Where(Node => !Node.InnerHtml.ToLower().Contains("toilet")).ToList();

            List<HtmlNode> CommentNodes = NodesAll.Where(Node => Node.NodeType == HtmlNodeType.Comment && Node.HasChildNodes).Where(Node => Node.InnerLength > 1).ToList();
            List<HtmlNode> ElementNodes = NodesAll.Where(Node => Node.NodeType == HtmlNodeType.Element && Node.HasChildNodes).Where(Node => Node.InnerLength > 1).ToList();
            List<HtmlNode> DocumentNodes = NodesAll.Where(Node => Node.NodeType == HtmlNodeType.Document && Node.HasChildNodes).Where(Node => Node.InnerLength > 1).ToList();
            List<HtmlNode> TextNodes = NodesAll.Where(Node => Node.NodeType == HtmlNodeType.Text && Node.HasChildNodes).Where(Node => Node.InnerLength > 1).ToList();


            if (ElementNodes is not null)
            {
                DoLog($"Got Element Nodes : {ElementNodes.Count}");
            }
            else
            {
                return;
            }

            if (CommentNodes is not null) { DoLog($"Got Comment Nodes : {CommentNodes.Count}"); }
            if (DocumentNodes is not null) { DoLog($"Got Document Nodes: {DocumentNodes.Count}"); }
            if (TextNodes is not null) { DoLog($"Got Text Nodes    : {TextNodes.Count}"); }

            MeshDoEvents();
            MeshDoEvents();
            MeshDoEvents();
            MeshDoEvents();

            for (int i = 0; i < ElementNodes.Count; i++)
            {
                HtmlNode Element = ElementNodes[i];

                HtmlNodeType NodeType = Element.NodeType;

                LogElement(Element);

                //IEnumerable<HtmlNode> Descendants   = Element.Descendants();
                IEnumerable<string> Classes = Element.GetClasses();

                //var IsEmptyElement = Node.IsEmptyElement();
                //var EncapsulatedData = Node.GetEncapsulatedData();

                string InnerText = Element.InnerText;
                string InnerHtml = Element.InnerHtml;
                string OuterHtml = Element.OuterHtml;

                DoLog($"Node {Convert.ToString(i + 1).PadLeft(4)} of {AllNodesCount}");
            }

            HtmlNodeCollection NodesVideo = HtmlDoc.DocumentNode.SelectNodes("//video");
            HtmlNodeCollection NodesDiv = HtmlDoc.DocumentNode.SelectNodes("//div");
            HtmlNodeCollection NodesImg = HtmlDoc.DocumentNode.SelectNodes("//img");
            HtmlNodeCollection NodesMp4 = HtmlDoc.DocumentNode.SelectNodes("//mp4");
            HtmlNodeCollection NodesInner = HtmlDoc.DocumentNode.SelectNodes("//inner");
            HtmlNodeCollection NodesOuter = HtmlDoc.DocumentNode.SelectNodes("//outer");

            if (NodesVideo is not null) { DoLog($"Got Video Nodes: {NodesVideo.Count}"); }
            if (NodesDiv is not null) { DoLog($"Got Div Nodes  : {NodesDiv.Count}"); }
            if (NodesImg is not null) { DoLog($"Got Image Nodes: {NodesImg.Count}"); }
            if (NodesMp4 is not null) { DoLog($"Got Mp4 Nodes  : {NodesMp4.Count}"); }
            if (NodesInner is not null) { DoLog($"Got Inner Nodes: {NodesInner.Count}"); }
            if (NodesOuter is not null) { DoLog($"Got Outer Nodes: {NodesOuter.Count}"); }

            for (int i = 0; i < AllNodesCount; i++)
            {
                var Node = NodesAll[i];

                string NodeLog = Node.InnerText.Trim();

                if (i > 7)
                {
                    DoLog($"Length: {Convert.ToString(Node.InnerText.Length).PadRight(8)} | Text: {Node.InnerText}");
                }

                if (Node.InnerText.Length < 3 || Node.InnerText == "\n\t")
                {
                    // Do Nothing
                    DoLog("");
                }
                else if (Node.InnerText.Contains("\n"))
                {
                    ParsedNode = SplitParseNode(Node);

                    DoLog(CommentLineBefore);
                    DoLog(ParsedNode);
                    DoLog(CommentLineAfter);
                }
                else if (Node.InnerText.Length > 30)
                {
                    string NodeText = string.Concat(Convert.ToChar(Node.InnerText.Split().Where(s => s.Length > 1)));

                    NodeLog = CommentLineBefore;
                    NodeLog += $"Iterating Nodes {i.ToString().PadLeft(4, Convert.ToChar("0"))} of {AllNodesCount}";
                    NodeLog += $"{CommentLineShort}";
                    NodeLog += $"{Node.InnerText.Substring(0, Node.InnerText.Length - 25).Replace("\t", "")}";
                    NodeLog += CommentLineAfter;

                    DoLog(CommentLineBefore);
                    DoLog(NodeText);
                    DoLog(CommentLineAfter);

                    MeshDoEvents();
                }
            }

            //HtmlNodeCollection NodesHref = doc.DocumentNode.SelectNodes("//a[@href]");

            List<HtmlNode> NodesRose = NodesHref.ToList().Where(Node => Node.OuterHtml.ToLower().Contains("www.javbangers.com/video/")).ToList();

            int count = NodesRose.Count();

            count = NodesHref.Count();

            for (int i = 0; i < count; i++)
            {
                HtmlNode Node = NodesHref[i];

                HtmlAttributeCollection Attributes = Node.Attributes;

                HtmlAttribute Attribute = Attributes["href"];

                foreach (string SubLink in Attribute.Value.Split(' '))
                {
                    if (SubLink.StartsWith("http") && !ListBoxLinks.Items.Contains(SubLink))
                    {
                        ListBoxLinks.Items.Add(SubLink);

                        //Hyperlinks.Add(SubLink);

                        DoLog(SubLink);
                    }
                }
            }
        }
        //.....................................................................

        //.....................................................................

    }
}


