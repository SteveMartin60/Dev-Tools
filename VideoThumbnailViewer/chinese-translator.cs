using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace VideoThumbnailViewer
{
    public partial class BaiduTranslator
    {
        private static readonly HttpClient _httpClient = new(new HttpClientHandler
        {
            UseCookies = true,
            AllowAutoRedirect = true
        });

        public static async Task<string> TranslateEnglishToChinese(string englishText)
        {
            try
            {
                // First load the main page to get cookies
                var homePageResponse = await _httpClient.GetAsync("https://fanyi.baidu.com/");

                homePageResponse.EnsureSuccessStatusCode();

                // Prepare the translation request
                string url = $"https://fanyi.baidu.com/mtpe-individual/multimodal?query={Uri.EscapeDataString(englishText)}&lang=en2zh";

                // Add headers to mimic a browser request
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
                _httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                _httpClient.DefaultRequestHeaders.Add("Referer", "https://fanyi.baidu.com/");
                _httpClient.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

                // Send GET request
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();

                // Parse the HTML response to find the translation
                // This is fragile and may break if Baidu changes their HTML structure
                var translationMatch = MyRegex1().Match(responseBody);

                if (translationMatch.Success)
                {
                    string translatedText = translationMatch.Groups[1].Value;
                    // Clean up the translation
                    translatedText = MyRegex().Replace(translatedText, "").Trim();
                    return translatedText;
                }

                return "Translation not found in response";
            }
            catch (Exception ex)
            {
                return $"Translation failed: {ex.Message}";
            }
        }

        [GeneratedRegex("<[^>]*>")]
        private static partial Regex MyRegex();
        [GeneratedRegex(@"class=""target-output""[^>]*>(.*?)<\/div>", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-US")]
        private static partial Regex MyRegex1();
    }
}

