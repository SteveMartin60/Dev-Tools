using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;

namespace VideoThumbnailViewer
{ 
    public class BaTranslator
    {
        private static async Task DoStuffMain(string[] args)
        {
            if (args.Length == 0)
            {
                Debug.WriteLine("Please provide text to translate as an argument.");
                return;
            }

            string textToTranslate = args[0];
            string translation = await TranslateTextAsync(textToTranslate);
            Console.WriteLine(translation);
        }

        public static async Task<string> TranslateTextAsync(string text)
        {
            string url = "https://cn.bing.com/ttranslatev3";

            var parameters = new Dictionary<string, string>
            {
                {"fromLang", "auto-detect"},  // Auto-detect source language
                {"text", text},
                {"to", "zh"},                 // Translate to Chinese
                {"tryFetchingGenderDebiasedTranslations", "true"}
            };

            using var client = new HttpClient();
            
            // Set headers to mimic browser request
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");

            var response = await client.PostAsync(url, new FormUrlEncodedContent(parameters));
            string jsonResponse = await response.Content.ReadAsStringAsync();

            // Simple parsing of the JSON response
            // Note: For robust parsing, you should use Newtonsoft.Json or System.Text.Json
            try
            {
                // The response is an array, get the first element
                int start = jsonResponse.IndexOf("\"translations\":") + 15;
                int end = jsonResponse.IndexOf("}]", start);
                string translationPart = jsonResponse[start..end];

                start = translationPart.IndexOf("\"text\":\"") + 8;
                end = translationPart.IndexOf('\"', start);
                return translationPart[start..end];
            }
            catch
            {
                return "Error parsing translation response: " + jsonResponse;
            }
        }
    }
}