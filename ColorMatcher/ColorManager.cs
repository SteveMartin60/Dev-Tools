using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ColorMatcher
{
    public class ColorManager
    {
        private const string ColorsCacheFile = "colors_cache.json";
        private const string CssColorsUrl = "https://raw.githubusercontent.com/meodai/color-names/master/dist/colornames.json";
        private const string PantoneColorsUrl = "https://github.com/import-this/pantone-colors/master/colors.json";
        private const string X11ColorsUrl = "https://raw.githubusercontent.com/import-this/x11-colors/master/colors.json";
        private const string MaterialColorsUrl = "https://raw.githubusercontent.com/import-this/material-colors/master/colors.json";

        private readonly Dictionary<string, string> _colorSetMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, Color> _allColors = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase);

        public bool ColorsLoaded => _allColors.Count > 0;

        public Dictionary<string, Color> GetAllColors() => new Dictionary<string, Color>(_allColors);

        public async Task DownloadCssColorsAsync()
        {
            try
            {
                _allColors.Clear();
                _colorSetMap.Clear();
                await DownloadAndMergeColors(CssColorsUrl, "CSS");
                SaveColorsToCache();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading CSS colors: {ex.Message}");
                throw;
            }
        }

        public async Task DownloadAllColorSetsAsync()
        {
            try
            {
                _allColors.Clear();
                _colorSetMap.Clear();

                //await Task.WhenAll(
                //    DownloadAndMergeColors(CssColorsUrl,      "CSS"     ),
                //    DownloadAndMergeColors(MaterialColorsUrl, "Material"),
                //    DownloadAndMergeColors(X11ColorsUrl,      "X11"     ),
                //    DownloadAndMergeColors(PantoneColorsUrl,  "Pantone" )
                //);

                await DownloadAndMergeColors(CssColorsUrl,      "CSS"     );
                //await DownloadAndMergeColors(MaterialColorsUrl, "Material");
                //await DownloadAndMergeColors(X11ColorsUrl,      "X11"     );
                //await DownloadAndMergeColors(PantoneColorsUrl,  "Pantone" );

                SaveColorsToCache();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading color sets: {ex.Message}");
                throw;
            }
        }

        private async Task DownloadAndMergeColors(string url, string setName)
        {
            try
            {
                using var httpClient = new HttpClient();
                var json = await httpClient.GetStringAsync(url);
                var colorDict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

                if (colorDict != null)
                {
                    foreach (var kv in colorDict)
                    {
                        var color = ParseHexColor(kv.Value);
                        if (!_allColors.ContainsKey(kv.Key))
                        {
                            _allColors.Add(kv.Key, color);
                            _colorSetMap[kv.Key] = setName;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading colors from {url}: {ex.Message}");
                throw;
            }
        }

        public Dictionary<string, Color> LoadAllColorSets()
        {
            try
            {
                if (!File.Exists(ColorsCacheFile))
                {
                    _allColors = new Dictionary<string, Color>();
                    return _allColors;
                }

                var json = File.ReadAllText(ColorsCacheFile);
                var cacheData = JsonSerializer.Deserialize<CachedColorData>(json);

                _allColors.Clear();
                _colorSetMap.Clear();

                if (cacheData?.Colors != null)
                {
                    foreach (var kv in cacheData.Colors)
                    {
                        _allColors[kv.Key] = ParseHexColor(kv.Value);
                    }
                }

                if (cacheData?.SetMapping != null)
                {
                    foreach (var mapping in cacheData.SetMapping)
                    {
                        _colorSetMap[mapping.Key] = mapping.Value;
                    }
                }

                return _allColors;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading cached colors: {ex.Message}");
                _allColors = new Dictionary<string, Color>();
                return _allColors;
            }
        }

        private void SaveColorsToCache()
        {
            try
            {
                var hexColorDict = _allColors.ToDictionary(
                    kv => kv.Key,
                    kv => $"#{kv.Value.R:X2}{kv.Value.G:X2}{kv.Value.B:X2}");

                var cacheData = new CachedColorData
                {
                    Colors = hexColorDict,
                    SetMapping = _colorSetMap
                };

                var json = JsonSerializer.Serialize(cacheData, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                File.WriteAllText(ColorsCacheFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving colors to cache: {ex.Message}");
                throw;
            }
        }

        private Color ParseHexColor(string hexColor)
        {
            if (string.IsNullOrEmpty(hexColor) || !hexColor.StartsWith("#"))
                return Colors.Transparent;

            try
            {
                hexColor = hexColor.TrimStart('#');
                if (hexColor.Length == 3)
                {
                    hexColor = $"{hexColor[0]}{hexColor[0]}{hexColor[1]}{hexColor[1]}{hexColor[2]}{hexColor[2]}";
                }
                else if (hexColor.Length != 6)
                {
                    return Colors.Transparent;
                }

                var r = Convert.ToByte(hexColor.Substring(0, 2), 16);
                var g = Convert.ToByte(hexColor.Substring(2, 2), 16);
                var b = Convert.ToByte(hexColor.Substring(4, 2), 16);
                return Color.FromRgb(r, g, b);
            }
            catch
            {
                return Colors.Transparent;
            }
        }

        public bool IsCssColor(string colorName) => _colorSetMap.TryGetValue(colorName, out var setName) && setName == "CSS";
        public bool IsMaterialColor(string colorName) => _colorSetMap.TryGetValue(colorName, out var setName) && setName == "Material";
        public bool IsX11Color(string colorName) => _colorSetMap.TryGetValue(colorName, out var setName) && setName == "X11";
        public bool IsPantoneColor(string colorName) => _colorSetMap.TryGetValue(colorName, out var setName) && setName == "Pantone";
    }

    public class CachedColorData
    {
        public Dictionary<string, string> Colors { get; set; } = new Dictionary<string, string>();
        public Dictionary<string, string> SetMapping { get; set; } = new Dictionary<string, string>();
    }
}
