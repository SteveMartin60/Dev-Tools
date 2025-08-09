using System.Diagnostics;

namespace WebScraper
{
    public partial class MainWindow
    {
        private void ParseLinkList(List<string> LinkList)
        {
            var PlayListSB = LinkList.Where(Link => Link.Contains("spankbang.com") && Link.Contains("/playlist/") && Link.LastIndexOf("/") < 40).ToList();
            var VideosSB = LinkList.Where(Link => Link.Contains("spankbang.com") && Link.Contains("/playlist/") && Link.LastIndexOf("/") > 40).ToList();

            for (int i = 0; i < PlayListSB.Count; i++)
            {
                string Line = PlayListSB[i];

                var L = Line.LastIndexOf("/");

                var T = Convert.ToString(Line.Substring(0, Line.LastIndexOf("/"))).PadRight(50);

                DoLog($"{Line}");
            }

        }
        //.....................................................................

        //.....................................................................
        private void ParseFaceList(List<string> LinkList)
        {
            var ListFaceSB = LinkList.Where(Link => !Link.Contains("\">") && !Link.ToLower().Contains("fart")).ToList();

            for (int i = 0; i < ListFaceSB.Count; i++)
            {
                string Line = ListFaceSB[i];

                var Url = Line.Split(" title=")[0];
                var Title = Line.Split(" title=")[1].PadRight(100);

                //var L = Line.LastIndexOf("/");

                //var T = Convert.ToString(Line.Substring(0, Line.LastIndexOf("/"))).PadRight(50);

                DoLog($"{Title} : {Url}");
            }

        }
        //.....................................................................

        //.....................................................................

    }
}


