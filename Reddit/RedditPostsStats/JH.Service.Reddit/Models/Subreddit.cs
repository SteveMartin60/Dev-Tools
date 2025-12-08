namespace JH.Service.Reddit.Models
{
    /// <summary>
    /// Reddit subreddit model, used to deserialize the JSON response from the Reddit API
    /// </summary>
    public class Subreddit
    {
        public string Before { get; set; }
        public string After { get; set; }
        public string Name { get; set; }
        public List<Post> Posts { get; set; }
    }
}