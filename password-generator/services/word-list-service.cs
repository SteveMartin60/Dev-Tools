using PasswordGenerator.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PasswordGenerator.Services
{
    public class WordListService
    {
        int Index = 0;
        
        List<string>? Adjectives     {get; set;}
        List<string>? LyAdjectives   {get; set;}
        List<string>? AdVerbs        {get; set;}
        List<string>? Nouns          {get; set;}
        List<string>? Verbs          {get; set;}
        List<string>? LyVerbs        {get; set;}

        List<string> superlatives = new List<string>();

        string AdjectiveList { get; set; } = @"D:\Dev-Tools\password-generator\word-lists\wordnet\dict\data.adj";
        string AdVerbList    { get; set; } = @"D:\Dev-Tools\password-generator\word-lists\wordnet\dict\data.adv";
        string NounList      { get; set; } = @"D:\Dev-Tools\password-generator\word-lists\wordnet\dict\data.noun";
        string VerbList      { get; set; } = @"D:\Dev-Tools\password-generator\word-lists\wordnet\dict\data.verb";

        int ColumnIndex = 0;
        public WordList GetDefaultWordLists(bool generateSuperlatives = false)
        {
            var wordList = new WordList();
            var filePath = "word-lists.txt";

            if (generateSuperlatives)
            {
                GenerateSuperlatives(AdjectiveList, @"D:\Dev-Tools\password-generator\word-lists\dict\data.super.txt");
            }

            LoadAdjectives();
            LoadLyAdjectives();
            LoadVerbs     ();
            LoadLyVerbs     ();
            LoadAdVerbs   ();
            LoadNouns     ();

            IEnumerable<string> lines = File.ReadAllLines(filePath).Skip(2);

            foreach (var line in lines.Skip(2))
            {
                ColumnIndex = 0;

                Index++;

                var columns = line.Split('|');

                int ColumnCount = columns.Count();

                if (columns.Count() > ColumnIndex)
                {
                    string ColumnString = columns[ColumnIndex];

                    AddWordFromColumn(wordList.LyVerbs, ColumnString);
                }
            }

            foreach (var line in lines.Skip(2))
            {
                ColumnIndex = 1;

                Index++;

                var columns = line.Split('|');

                int ColumnCount = columns.Count();

                if (columns.Count() > ColumnIndex)
                {
                    string ColumnString = columns[ColumnIndex];

                    AddWordFromColumn(wordList.Verbs, ColumnString);
                }
            }

            foreach (var line in lines.Skip(2))
            {
                ColumnIndex = 2;

                Index++;

                var columns = line.Split('|');

                int ColumnCount = columns.Count();

                if (columns.Count() > ColumnIndex)
                {
                    string ColumnString = columns[ColumnIndex];

                    AddWordFromColumn(wordList.Adjectives, ColumnString);
                }
            }

            foreach (var line in lines.Skip(2))
            {
                ColumnIndex = 3;

                Index++;

                var columns = line.Split('|');

                int ColumnCount = columns.Count();

                if (columns.Count() > ColumnIndex)
                {
                    string ColumnString = columns[ColumnIndex];

                    AddWordFromColumn(wordList.Nouns, ColumnString);
                }
            }

            foreach (var line in lines.Skip(2))
            {
                ColumnIndex = 4;

                Index++;

                var columns = line.Split('|');

                int ColumnCount = columns.Count();

                if (columns.Count() > ColumnIndex)
                {
                    string ColumnString = columns[ColumnIndex];

                    AddWordFromColumn(wordList.Superlatives, ColumnString);
                }
            }

            return wordList;
        }
        //.....................................................................

        //.....................................................................
        private void AddWordFromColumn(List<string> wordList, string column)
        {
            try
            {
                Debug.WriteLine("Check here");

                var P = column.Trim().Split([' ', '\t']);

                var parts = column.Trim().Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0 && !string.IsNullOrWhiteSpace(parts[0]))
                {
                    wordList.Add(parts[0]);
                }
            }
            catch (Exception)
            {
                throw;
            }

        }
        //.....................................................................

        //.....................................................................
        private void LoadAdjectives()
        {
            List<string> AdjectivesLines = File.ReadAllLines(AdjectiveList).Skip(29).ToList();
            
            Adjectives = new List<string>();

            for (int i = 0; i < AdjectivesLines.Count; i++)
            {
                var Parts = AdjectivesLines[i].Split(' ');

                var word = Parts[4].Trim();

                if (FilterWordString(word, false, 4, 6) && !Adjectives.Contains(word))
                {
                    Adjectives.Add(word);
                }
            }

            Debug.WriteLine("Loaded Adjectives");
        }
        //.....................................................................

        //.....................................................................
        private void LoadLyAdjectives()
        {
            LyAdjectives = new List<string>();

            List<string> AdjectivesLines = File.ReadAllLines(AdjectiveList).Skip(29).ToList();
            
            Adjectives = new List<string>();

            for (int i = 0; i < AdjectivesLines.Count; i++)
            {
                var Parts = AdjectivesLines[i].Split(' ');

                var word = Parts[4].Trim();

                if (FilterWordString(word, true, 4, 6) && !LyAdjectives.Contains(word))
                {
                    LyAdjectives.Add(word);
                }
            }

            Debug.WriteLine("Loaded Adjectives");
        }
        //.....................................................................

        //.....................................................................
        private void LoadAdVerbs()
        {
            List<string> AdVerbLines = File.ReadAllLines(AdVerbList).Skip(29).ToList();
            
            AdVerbs = new List<string>();

            for (int i = 0; i < AdVerbLines.Count; i++)
            {
                var Parts = AdVerbLines[i].Split(' ');

                var word = Parts[4].Trim();

                if (FilterWordString(word, false, 4, 6) && !AdVerbs.Contains(word))
                { 
                    AdVerbs.Add(word);
                }
            }

            Debug.WriteLine("Loaded AdVerbs");
        }
        //.....................................................................

        //.....................................................................
        private void LoadNouns()
        {
            List<string> NounLines = File.ReadAllLines(NounList).Skip(29).ToList();
            
            Nouns = new List<string>();

            for (int i = 0; i < NounLines.Count; i++)
            {
                var Parts = NounLines[i].Split(' ');

                var word = Parts[4].Trim();

                if (FilterWordString(word, false, 4, 6) && !Nouns.Contains(word))
                { 
                    Nouns.Add(word);
                }
            }

            Debug.WriteLine("Loaded Nouns");
        }
        //.....................................................................

        //.....................................................................
        private void LoadVerbs()
        {
            List<string> VerbLines = File.ReadAllLines(VerbList).Skip(29).ToList();
            
            Verbs = new List<string>();

            for (int i = 0; i < VerbLines.Count; i++)
            {
                var Parts = VerbLines[i].Split(' ');

                var word = Parts[4].Trim();

                if (FilterWordString(word, false, 4, 6) && !Verbs.Contains(word))
                { 
                    Verbs.Add(word);
                }
            }

            Debug.WriteLine("Loaded Verbs");
        }
        //.....................................................................

        //.....................................................................
        private void LoadLyVerbs()
        {
            List<string> VerbLines = File.ReadAllLines(VerbList).Skip(29).ToList();

            LyVerbs = new List<string>();

            for (int i = 0; i < VerbLines.Count; i++)
            {
                var Parts = VerbLines[i].Split(' ');

                var word = Parts[4].Trim();

                if (FilterWordString(word, true, 4, 6) && !LyVerbs.Contains(word))
                { 
                    LyVerbs.Add(word);
                }
            }

            Debug.WriteLine("Loaded Verbs");
        }
        //.....................................................................

        //.....................................................................
        private bool FilterWordString(string Word, bool LyOnly, int MinLength, int MaxLength)
        {
            bool Short = Word.Length < MinLength;
            bool Long  = Word.Length > MaxLength;
            bool Pass  = Word.Contains("_") || Word.Contains(".") || Word.Contains("-") ||  Word.Contains("'") ||  Word.Contains("-");

            bool IsLy = Word.EndsWith("ly");

            bool Result = !Short && !Long && !Pass;

            if (LyOnly)
            {
                Result = Result && IsLy;
            }

            return Result;
        }
        //.....................................................................
        #region Begin Region Superlatives
        //.....................................................................
        public void GenerateSuperlatives(string inputFilePath, string outputFilePath)
        {
            var consecutiveConsonantsPattern = new Regex(@"^([bcdfghjkilmnpqrstvwxyz])\1", RegexOptions.IgnoreCase);
            
            var irregulars = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "good", "best" },
            { "bad", "worst" },
            { "far", "farthest" },
            { "little", "least" }
        };

            // List to store superlative forms
            var superlatives = new List<string>();

            // Regex for consonant-vowel-consonant (CVC) pattern
            var cvcPattern = new Regex(@"[bcdfghjklmnpqrstvz][aeiou][bcdfghjklmnpqrstvz]$", RegexOptions.IgnoreCase);

            try
            {
                // Read data.adj file
                string[] lines = File.ReadAllLines(inputFilePath);

                foreach (string line in lines)
                {
                    // Skip comment lines starting with spaces
                    if (line.StartsWith("  "))
                        continue;

                    if (line.StartsWith("x"))
                        continue;

                    if (line.Contains("."))
                        continue;

                    if (line.Contains("'"))
                        continue;

                    if (line.Contains("-"))
                        continue;

                    if (line.Contains("cx"))
                        continue;

                    if (line.Contains("lx"))
                        continue;

                    if (line.Contains("ii"))
                        continue;

                    // Split line into parts
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length < 4)
                        continue;

                    // Check POS (a = adjective, s = adjective satellite)
                    string pos = parts[2];
                    if (pos != "a" && pos != "s")
                        continue;

                    // Get word count (hexadecimal)
                    if (!int.TryParse(parts[3], NumberStyles.HexNumber, null, out int wordCount))
                        continue;

                    // Extract each word in the synset
                    for (int i = 0; i < wordCount; i++)
                    {
                        int wordIndex = 4 + i * 2; // Words are at positions 4, 6, 8, ...
                        if (wordIndex >= parts.Length)
                            break;

                        string word = parts[wordIndex].ToLower();

                        // Skip words with underscores, starting with a digit, or starting with consecutive identical consonants
                        if (word.Contains("_") || char.IsDigit(word[0]) || consecutiveConsonantsPattern.IsMatch(word))
                            continue;
                        
                        // Skip words with underscores (multi-word phrases) or starting with a digit
                        if (word.Contains("_") || char.IsDigit(word[0]))
                            continue;

                        // Generate superlative
                        string superlative;
                        if (irregulars.ContainsKey(word))
                        {
                            superlative = irregulars[word];
                        }
                        else if (Regex.IsMatch(word, @"[aeiou]?y$") && !Regex.IsMatch(word, @"[aeiou]y$"))
                        {
                            // Two-syllable adjectives ending in -y (e.g., happy → happiest)
                            superlative = word.Substring(0, word.Length - 1) + "iest";
                        }
                        else if (word.Length <= 5 && cvcPattern.IsMatch(word) && !word.EndsWith("w") && !word.EndsWith("x") && !word.EndsWith("y"))
                        {
                            // One-syllable CVC adjectives (e.g., fat → fattest, mad → maddest)
                            superlative = word + word[^1] + "est"; // Double last consonant
                        }
                        else if (word.Length <= 5)
                        {
                            // Other one-syllable adjectives (e.g., wet → wettest)
                            superlative = word + "est";
                        }
                        else
                        {
                            // Multi-syllable adjectives (e.g., beautiful → most beautiful)
                            superlative = "most " + word;
                        }

                        if (FilterWordString(superlative, false, 4, 7) && !superlatives.Contains(superlative))
                        {
                            superlatives.Add(superlative);
                        }
                    }
                }

                // Write to output file (superlative only)
                using (StreamWriter writer = new StreamWriter(outputFilePath, false))
                {
                    foreach (string sup in superlatives.OrderBy(s => s))
                    {
                        writer.WriteLine(sup);
                    }
                }

                Debug.WriteLine($"Superlatives written to {outputFilePath}. Total entries: {superlatives.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file: {ex.Message}");
            }
        }
        //.....................................................................
        #endregion End Region Superlatives
        //.....................................................................
    }
}
