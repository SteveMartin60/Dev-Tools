using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PasswordGenerator.Models
{
    public class PassphraseGenerator
    {
        private readonly WordList _wordList;
        private readonly Random _random = new Random();

        public PassphraseGenerator(WordList wordList)
        {
            _wordList = wordList;
        }

        public string GeneratePassphrase()
        {
            // 1. Identify possible starting letters that exist in all required categories
            var possibleLetters = GetPossibleStartingLetters();

            if (possibleLetters.Count == 0)
            {
                return "No valid starting letters found across all word lists";
            }

            // 2. Pick a random starting letter
            var selectedLetter = possibleLetters[_random.Next(possibleLetters.Count)];

            // 3. Select words for each category that start with the chosen letter
            var words = new List<string>
            {
                GetRandomWordStartingWith(_wordList.LyVerbs,      selectedLetter),
                GetRandomWordStartingWith(_wordList.Verbs,        selectedLetter),
                GetRandomWordStartingWith(_wordList.Superlatives, selectedLetter),
                GetRandomWordStartingWith(_wordList.Adjectives,   selectedLetter),
                GetRandomWordStartingWith(_wordList.Nouns,        selectedLetter),
                GetRandomWordStartingWith(_wordList.LyVerbs,      selectedLetter)
            };

            // Capitalize exactly two random words
            var indicesToCapitalize = GetRandomIndices(words.Count, 2);
            for (int i = 0; i < words.Count; i++)
            {
                if (indicesToCapitalize.Contains(i))
                {
                    words[i] = CapitalizeFirstLetter(words[i]);
                }
            }

            return string.Join("-", words).Trim();
        }

        private List<char> GetPossibleStartingLetters()
        {
            // Get all letters that have at least one word in each required category
            var lettersWithLyVerbs      = GetStartingLetters(_wordList.LyVerbs     );
            var lettersWithVerbs        = GetStartingLetters(_wordList.Verbs       );
            var lettersWithSuperlatives = GetStartingLetters(_wordList.Superlatives);
            var lettersWithAdjectives   = GetStartingLetters(_wordList.Adjectives  );
            var lettersWithNouns        = GetStartingLetters(_wordList.Nouns       );

            // Find intersection of all letter sets
            return lettersWithLyVerbs
                .Intersect(lettersWithVerbs)
                .Intersect(lettersWithSuperlatives)
                .Intersect(lettersWithAdjectives)
                .Intersect(lettersWithNouns)
                .ToList();
        }

        private HashSet<char> GetStartingLetters(List<string> words)
        {
            var Result= new HashSet<char>(words
                .Where(w => !string.IsNullOrEmpty(w))
                .Select(w => char.ToUpper(w[0])));

            Debug.WriteLine("Got starting letters");

            return Result;
        }

        private string GetRandomWordStartingWith(List<string> words, char letter)
        {
            var matchingWords = words
                .Where(w => w.Length > 0 && char.ToUpper(w[0]) == char.ToUpper(letter))
                .ToList();

            return matchingWords.Count > 0
                ? matchingWords[_random.Next(matchingWords.Count)]
                : $"no_{letter}_match";
        }

        private List<int> GetRandomIndices(int totalCount, int countToSelect)
        {
            var indices = Enumerable.Range(0, totalCount).ToList();
            var selected = new List<int>();

            for (int i = 0; i < countToSelect; i++)
            {
                var randomIndex = _random.Next(indices.Count);
                selected.Add(indices[randomIndex]);
                indices.RemoveAt(randomIndex);
            }

            return selected;
        }

        private string CapitalizeFirstLetter(string word)
        {
            if (string.IsNullOrEmpty(word))
                return word;

            return char.ToUpper(word[0]) + word.Substring(1).ToLower();
        }
    }
}
