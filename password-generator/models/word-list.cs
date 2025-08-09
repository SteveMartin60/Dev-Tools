namespace PasswordGenerator.Models
{
    public class WordList
    {
        public List<string> LyVerbs      { get; set; } = new List<string>();
        public List<string> Verbs        { get; set; } = new List<string>();
        public List<string> Superlatives { get; set; } = new List<string>();
        public List<string> Adjectives   { get; set; } = new List<string>();
        public List<string> Nouns        { get; set; } = new List<string>();
    }
}
