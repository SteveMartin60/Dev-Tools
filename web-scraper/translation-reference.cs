using System;
using System.Collections.Generic;

namespace WebScraper
{
    /// <summary>
    /// Immutable reference terms for deterministic translation validation.
    /// </summary>
    public static class TranslationReferenceTerms
    {
        /// <summary>
        /// Dictionary of English terms and their exact Chinese translations.
        /// </summary>
        public static readonly IReadOnlyDictionary<string, string> Terms = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Astroparticle Physics"] = "天体粒子物理学",
            ["Neutrino Phenomenology"] = "中微子现象学",
            ["Monte Carlo Event Generators"] = "蒙特卡洛事件生成器",
            ["Gravitational Wave Physics"] = "引力波物理学",
            ["Chiral Perturbation Theory"] = "手征微扰理论",
            ["Non-Abelian Gauge Theory"] = "非阿贝尔规范理论",
            ["Topological Insulator"] = "拓扑绝缘体"
        };

        /// <summary>
        /// Validates if a translated term matches the expected reference.
        /// </summary>
        /// <param name="englishTerm">Case-insensitive English term.</param>
        /// <param name="translatedTerm">Candidate Chinese translation.</param>
        /// <returns>True if the translation matches exactly.</returns>
        public static bool IsValidTranslation(string englishTerm, string translatedTerm)
        {
            if (Terms.TryGetValue(englishTerm, out string expectedTranslation))
            {
                return string.Equals(translatedTerm, expectedTranslation, StringComparison.Ordinal);
            }
            throw new KeyNotFoundException($"Term '{englishTerm}' not found in reference dictionary.");
        }

        /// <summary>
        /// Returns all reference terms for iteration.
        /// </summary>
        public static IEnumerable<KeyValuePair<string, string>> GetAllTerms() => Terms;
    }
}
