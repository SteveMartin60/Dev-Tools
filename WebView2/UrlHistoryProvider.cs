// ======================================================================
// UrlHistoryProvider.cs
// ======================================================================
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace WebView2Browser
{
    public sealed class UrlHistoryProvider
    {
        private readonly ObservableCollection<string> _items = new();
        private static readonly string HistoryPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "WebView2Browser", "history.json");

        public ReadOnlyObservableCollection<string> Items { get; }

        public UrlHistoryProvider()
        {
            Items = new ReadOnlyObservableCollection<string>(_items);
            Load();
        }

        public void Add(string url)
        {
            if (string.IsNullOrWhiteSpace(url) || url.StartsWith("about:")) return;

            _items.Remove(url);
            _items.Insert(0, url);

            // Trim to 200 items
            while (_items.Count > 200)
                _items.RemoveAt(_items.Count - 1);

            Save();
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(HistoryPath)) return;
                var list = JsonSerializer.Deserialize<string[]>(File.ReadAllText(HistoryPath));
                foreach (var u in list ?? Array.Empty<string>())
                    _items.Add(u);
            }
            catch { /* ignore corrupt file */ }
        }

        private void Save()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(HistoryPath)!);
                File.WriteAllText(HistoryPath,
                    JsonSerializer.Serialize(_items.ToArray(), new JsonSerializerOptions { WriteIndented = true }));
            }
            catch { /* ignore I/O errors */ }
        }
    }
}
