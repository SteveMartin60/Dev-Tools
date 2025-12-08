namespace JH.Service.Reddit.Models
{
    /// <summary>
    /// Reddit response model, used to deserialize the JSON response from the Reddit API
    /// </summary>
    public class RedditResponse
    {
        public string access_token { get; set; } = string.Empty;
        public string token_type { get; set; } = string.Empty;
        public int expires_in { get; set; }
        public string scope { get; set; } = string.Empty;
        public string refresh_token { get; set; } = string.Empty;
        public string error { get; set; } = string.Empty;
    }
}
