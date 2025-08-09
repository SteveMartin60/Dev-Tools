using AngleSharp.Browser;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;

namespace WebView2Browser
{
    public partial class MainWindow : Window
    {
        private async Task CaptureHtml()
        {
            try
            {
                var capturedLines = await HtmlCaptureService.CaptureHtmlToFile();

                var results = capturedLines
                    .Where(line => line.Contains("<a href=\"/") && line.Contains("title") && line.Contains("/video/"))
                    .ToList();

                for (int i = 0; i < capturedLines.Count; i++)
                {
                    var line = capturedLines[i];

                    if (line.Contains("<a>...</a>"))
                    {
                        int Index = i;

                        MaxPageCount = Convert.ToInt32(capturedLines[Index + 3].Split(">")[1].Split("<")[0]);

                        Debug.WriteLine(MaxPageCount);
                    }
                }

                for (int i = 0; i < results.Count; i++)
                {
                    results[i] = results[i].Replace("<a href=\"/", SiteAddress).Replace(">", "").Trim();
                }

                Links.AddRange(results);

                File.WriteAllLines(@$"{LinksSavePath}FaceSitting-Links.txt", Links);

                foreach (var line in results)
                {
                    Debug.WriteLine(line.Trim());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing HTML: {ex.Message}");
            }
        }

        private async Task<List<string>> CaptureLines()
        {
            List<string> capturedLines = await HtmlCaptureService.CaptureHtmlToFile();
            return capturedLines;
        }

        private async Task SetMaxPageCount()
        {
            var capturedLines = await CaptureLines();

            for (int i = 0; i < capturedLines.Count; i++)
            {
                var line = capturedLines[i];

                if (line.Contains("<a>...</a>"))
                {
                    int Index = i;

                    MaxPageCount = Convert.ToInt32(capturedLines[Index + 3].Split(">")[1].Split("<")[0]);

                    Debug.WriteLine(MaxPageCount);
                }
            }
        }

        private async Task DoCapture(bool DoSubPages = true)
        {
            await SetMaxPageCount();

            while (CurrentPageIndex < MaxPageCount)
            {
                await CaptureHtml();

                CurrentAddress = BaseUrl.Replace("?q=fhd", "") + CurrentPageIndex++ + "/?q=fhd";

                if (string.IsNullOrEmpty(BaseUrl))
                {
                    BaseUrl = CurrentAddress;
                }

                await NavigationHandler.NavigateToAddressAsync(CurrentAddress);
            }
        }

        private async void CaptureHtmlButton_Click(object sender, RoutedEventArgs e)
        {
            await DoCapture();
        }
    }
}
