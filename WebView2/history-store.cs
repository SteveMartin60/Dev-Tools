using System.IO;
using System.Text.Json;

namespace WebView2Browser
{
    public static class HistoryStore
    {
        private static readonly string PathToFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WebView2Browser", "history.json");

        public static void Add(string url)
        {
            if (string.IsNullOrWhiteSpace(url) || url.StartsWith("about:")) 
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(PathToFile)!);

            var list = Load();
            list.Remove(url);        // move to top
            list.Insert(0, url);
            list = list.Distinct().Take(200).ToList();

            File.WriteAllText(PathToFile, JsonSerializer.Serialize(list));
        }

        public static List<string> Load()
        {
            return File.Exists(PathToFile)
                ? JsonSerializer.Deserialize<List<string>>(File.ReadAllText(PathToFile))!
                : new List<string>();
        }
    }
}
