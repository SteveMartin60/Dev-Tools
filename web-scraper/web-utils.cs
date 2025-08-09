using System.Diagnostics;
using System.Net;

namespace WebScraper
{
    public partial class MainWindow
    {
        private bool UrlReachable(string url)
        {
            bool result = false;

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Timeout = 15000;
            request.Method = "HEAD";
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    HttpStatusCode StatusCode = response.StatusCode;

                    if (StatusCode == HttpStatusCode.OK)
                    {
                        //DoLog($"Valid Link: {url}");

                        result = true;
                    }
                    else
                    {
                        result = false;
                    }
                }
            }
            catch (WebException)
            {
                result = false;
            }

            return result;
        }
        //.....................................................................

        //.....................................................................
        private bool ValidateUrl(string WebPageAddress)
        {
            //DoLog("Validate URL");

            bool Result = true;

            if (!Uri.IsWellFormedUriString(WebPageAddress, UriKind.Absolute))
            {
                Result = false;
            }

            if (Result)
            {
                Result = UrlReachable(WebPageAddress);
            }
            else
            {
                DoLog($"Dodgy URL: {WebPageAddress}");
            }

            if (Result)
            {
                Result = NodesHrefFiltered.Count > 8;
            }

            return Result;
        }
        //.....................................................................

        //.....................................................................
    }
}


