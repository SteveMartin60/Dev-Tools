using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;

namespace WordNetParserApp
{
    public partial class MainWindow : Window
    {
        private readonly string dictPath = @"D:\Dev-Tools\password-generator\word-lists\wordnet\dict\";
        private readonly string glossTagPath = @"D:\Dev-Tools\password-generator\word-lists\wordnet\glosstag\merged\";
        private readonly Dictionary<string, (string FileName, bool IsGlossTag)> fileMap = new Dictionary<string, (string, bool)>
        {
            { "Noun", ("index.noun", false) },
            { "Verb", ("index.verb", false) },
            { "Adjective (JJ)", ("index.adj", false) },
            { "Adverb (RB)", ("index.adv", false) },
            { "Comparative Adverb (RBR)", ("index.adv", false) },
            { "Superlative Adverb (RBS)", ("adv.xml", true) },
            { "Superlative Adjective (JJS)", ("adj.xml", true) }
        };
        private readonly HashSet<string> irregularSuperlatives = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "best", "most", "worst", "least", "farthest", "furthest"
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private void PartOfSpeechComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PartOfSpeechComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                string partOfSpeech = selectedItem.Content.ToString();
                LoadWords(partOfSpeech);
            }
        }

        private void LoadWords(string partOfSpeech)
        {
            WordsListBox.Items.Clear();
            StatusTextBlock.Text = "Loading words...";

            try
            {
                if (!fileMap.ContainsKey(partOfSpeech))
                {
                    StatusTextBlock.Text = "Invalid part of speech selected.";
                    StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                var (fileName, isGlossTag) = fileMap[partOfSpeech];
                List<string> words = new List<string>();
                string filePath = isGlossTag ? Path.Combine(glossTagPath, fileName) : Path.Combine(dictPath, fileName);

                if (!File.Exists(filePath))
                {
                    StatusTextBlock.Text = $"File not found: {filePath}";
                    StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                    return;
                }

                if (isGlossTag)
                {
                    // Parse glosstag XML files for RBS and JJS
                    try
                    {
                        XDocument doc = XDocument.Load(filePath);
                        var synsets = doc.Descendants().Where(e => e.Name.LocalName == "synset");
                        int synsetCount = synsets.Count();
                        int lemmaCount = 0;

                        foreach (XElement synset in synsets)
                        {
                            // Try multiple possible lemma element names
                            string lemma = null;
                            var lemmaElement = synset.Element("lemma") ?? synset.Element("term") ?? synset.Element("word");
                            if (lemmaElement != null)
                            {
                                lemma = lemmaElement.Value.Replace("_", " ");
                            }

                            // Try multiple possible sense key attributes
                            string senseKey = null;
                            var senseElement = synset.Element("sense") ?? synset.Element("wf");
                            if (senseElement != null)
                            {
                                senseKey = senseElement.Attribute("sensekey")?.Value ??
                                          senseElement.Attribute("sk")?.Value;
                            }

                            if (string.IsNullOrEmpty(lemma)) continue;

                            lemmaCount++;
                            bool isSuperlative = false;

                            // Check for superlative markers
                            if (!string.IsNullOrEmpty(senseKey) && senseKey.Contains("(p)"))
                            {
                                isSuperlative = true;
                            }
                            // Heuristic: check for -est ending
                            else if (lemma.EndsWith("est", StringComparison.OrdinalIgnoreCase))
                            {
                                isSuperlative = true;
                            }
                            // Check for irregular superlatives
                            else if (irregularSuperlatives.Contains(lemma))
                            {
                                isSuperlative = true;
                            }

                            if (isSuperlative)
                            {
                                words.Add(lemma);
                            }
                        }

                        StatusTextBlock.Text = $"Parsed {synsetCount} synsets, found {lemmaCount} lemmas, loaded {words.Count} {partOfSpeech.ToLower()}s.";
                    }
                    catch (Exception ex)
                    {
                        // Fallback to index files if glosstag parsing fails
                        StatusTextBlock.Text = $"Failed to parse glosstag file: {ex.Message}. Falling back to index file.";
                        StatusTextBlock.Foreground = System.Windows.Media.Brushes.Orange;
                        filePath = Path.Combine(dictPath, partOfSpeech == "Superlative Adverb (RBS)" ? "index.adv" : "index.adj");
                        words = ParseIndexFile(filePath, partOfSpeech, true);
                    }
                }
                else
                {
                    // Parse index files for Noun, Verb, JJ, RB, RBR
                    words = ParseIndexFile(filePath, partOfSpeech, false);
                }

                // Sort words alphabetically and add to ListBox
                words = words.Distinct().OrderBy(w => w).ToList();
                foreach (string word in words)
                {
                    WordsListBox.Items.Add(word);
                }

                StatusTextBlock.Text = $"Loaded {words.Count} {partOfSpeech.ToLower()}s.";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Green;
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Error loading words: {ex.Message}";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
            }
        }

        private List<string> ParseIndexFile(string filePath, string partOfSpeech, bool isSuperlativeFallback)
        {
            List<string> words = new List<string>();
            if (!File.Exists(filePath))
            {
                StatusTextBlock.Text = $"Index file not found: {filePath}";
                StatusTextBlock.Foreground = System.Windows.Media.Brushes.Red;
                return words;
            }

            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip comment lines starting with spaces
                    if (line.StartsWith("  ")) continue;

                    // The lemma is the first field in the index file
                    string[] fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (fields.Length < 2) continue;

                    string lemma = fields[0].Replace("_", " "); // Replace underscores with spaces
                    bool includeWord = false;

                    if (partOfSpeech == "Noun" || partOfSpeech == "Verb" ||
                        partOfSpeech == "Adjective (JJ)" || partOfSpeech == "Adverb (RB)")
                    {
                        includeWord = true;
                    }
                    else if (partOfSpeech == "Comparative Adverb (RBR)")
                    {
                        if (lemma.EndsWith("er", StringComparison.OrdinalIgnoreCase) ||
                            fields.Any(f => f.Contains("(r)")))
                        {
                            includeWord = true;
                        }
                    }
                    else if (isSuperlativeFallback &&
                             (partOfSpeech == "Superlative Adverb (RBS)" || partOfSpeech == "Superlative Adjective (JJS)"))
                    {
                        if (lemma.EndsWith("est", StringComparison.OrdinalIgnoreCase) ||
                            fields.Any(f => f.Contains("(p)")) ||
                            irregularSuperlatives.Contains(lemma))
                        {
                            includeWord = true;
                        }
                    }

                    if (includeWord)
                    {
                        words.Add(lemma);
                    }
                }
            }

            return words;
        }
    }
}
