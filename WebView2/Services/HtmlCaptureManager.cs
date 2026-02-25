using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebView2Browser
{
    public class HtmlCaptureManager
    {
        private readonly Services.HtmlCaptureService _htmlCaptureService;
        private readonly Core.WebViewNavigationHandler _navigationHandler;

        public List<string> Links { get; } = new List<string>();
        public string BaseUrl { get; set; }
        public string CurrentAddress { get; set; }
        public int MaxPageCount { get; set; }
        public string LinksSavePath { get; set; }
        public int CurrentPageIndex { get; set; } = 1;

        public HtmlCaptureManager(Services.HtmlCaptureService htmlCaptureService, Core.WebViewNavigationHandler navigationHandler, string linksSavePath)
        {
            _htmlCaptureService = htmlCaptureService;
            _navigationHandler = navigationHandler;
            LinksSavePath = linksSavePath;
        }

        public async Task CaptureAndProcessHtml()
        {
            if (string.IsNullOrEmpty(BaseUrl))
            {
                BaseUrl = CurrentAddress;
            }

            var capturedLines = await _htmlCaptureService.CaptureHtmlToFile();
            ProcessCapturedHtml(capturedLines);

            CurrentAddress = $"{BaseUrl.Replace("?q=fhd", "")}{CurrentPageIndex++}/?q=fhd";
            await _navigationHandler.NavigateToAddressAsync(CurrentAddress);
        }

        private void ProcessCapturedHtml(List<string> capturedLines)
        {
            var results = capturedLines
                .Where(line => line.Contains("<a href=\"/") && line.Contains("title") && line.Contains("/video/"))
                .Select(line => line.Replace("<a href=\"/", "https://mysite.com/").Replace(">", "").Trim())
                .ToList();

            Links.AddRange(results);
            SaveLinksToFile();
            LogResults(results);
            FindMaxPageCount(capturedLines);
        }

        private void SaveLinksToFile()
        {
            File.WriteAllLines($@"{LinksSavePath}FaceSitting-Links.txt", Links);
        }

        private void LogResults(IEnumerable<string> results)
        {
            foreach (var line in results)
            {
                Debug.WriteLine(line.Trim());
            }
        }

        private void FindMaxPageCount(List<string> capturedLines)
        {
            for (int i = 0; i < capturedLines.Count; i++)
            {
                if (capturedLines[i].Contains("<a>...</a>"))
                {
                    MaxPageCount = Convert.ToInt32(capturedLines[i + 3].Split(">")[1].Split("<")[0]);
                    Debug.WriteLine(MaxPageCount);
                    break;
                }
            }
        }
    }
}
