using JH.Service.Reddit.Models;
using Newtonsoft.Json.Linq;

namespace JH.Service.Reddit
{
    /// <summary>
    /// This class is responsible for making the HTTP requests to the Reddit API, and parsing the response into a Subreddit object and a list of Post objects.
    /// This will continue to make requests until the entire subreddit has been retrieved.
    ///
    /// This will use rate limit, but at this time Reddit is not sending the rate limit headers
    /// </summary>
    public class RedditService : IRedditService
    {
        private readonly HttpClient _client;
        private static System.Timers.Timer _requestTimer;
        private Subreddit subreddit;
        private int _rateLimitUsed;
        private int _rateLimitRemaining;
        private int _rateLimitReset;
        private string _lastBefore = "t3_190w6l5"; // simulate the first request

        // Define an event that clients can subscribe to
        public event EventHandler<SubredditEventArgs> SubredditUpdated;

        public RedditService(HttpClient client)
        {
            _client = client;
        }

        public RedditService()
        {
            _client = new HttpClient();
        }

        /// <summary>
        /// Get the subreddit and related posts for the given subreddit name
        /// </summary>
        /// <param name="subredditName">The name of the subreddit to retrieve</param>
        /// <param name="before">The ID of the post to start at</param>
        /// <param name="limit">The number of posts to retrieve per request</param>
        public async Task<Subreddit> GetSubredditAsync(string subredditName, string before = null, int limit = 100)
        {
            // Check if subredditName is null or empty
            if (string.IsNullOrEmpty(subredditName))
            {
                throw new ArgumentException("Subreddit name cannot be null or empty.", nameof(subredditName));
            }

            HttpResponseMessage response = null;

            try
            {
                subreddit = new()
                {
                    Name = subredditName,
                    Posts = new List<Post>()
                };

                string subRedditUrl = $"https://www.reddit.com/r/{subredditName}/.json?limit={limit}";
                if (!string.IsNullOrEmpty(before))
                    subRedditUrl += $"&before={before}";

                response = await _client.GetAsync(subRedditUrl);

                try
                {
                    response.EnsureSuccessStatusCode();
                    // Read rate limit headers
                    _rateLimitUsed = int.Parse(response.Headers.GetValues("X-RateLimit-Used").FirstOrDefault());
                    _rateLimitRemaining = int.Parse(response.Headers.GetValues("X-RateLimit-Remaining").FirstOrDefault());
                    _rateLimitReset = int.Parse(response.Headers.GetValues("X-RateLimit-Reset").FirstOrDefault());
                }
                catch (Exception)
                {   // currently these headers are not returned, I believe Reddit is working on it
                    _rateLimitRemaining = 0;
                    _rateLimitReset = 0;
                    _rateLimitUsed = 0;
                }

                if (response.IsSuccessStatusCode)
                {
                    // read the response content and populate the subreddit object
                    var content = await response.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(content);

                    var jArray = jObject["data"]["children"] as JArray;

                    foreach (var obj in jArray)
                    {
                        Post post = new()
                        {
                            Id = obj["data"]["id"].ToString(),
                            Title = obj["data"]["title"].ToString(),
                            Author = obj["data"]["author"].ToString(),
                            Score = obj["data"]["score"].Value<int>(),
                            Thumbnail = obj["data"]["thumbnail"].ToString(),
                            Url = obj["data"]["url"].ToString(),
                            NumberOfComments = obj["data"]["num_comments"].Value<int>(),
                            CreatedUTC = DateTimeOffset.FromUnixTimeSeconds(obj["data"]["created"].Value<long>()).DateTime,
                            Created = DateTimeOffset.FromUnixTimeSeconds(obj["data"]["created_utc"].Value<long>()).DateTime
                        };

                        subreddit.Posts.Add(post);
                    }

                    if (subreddit.Posts.Count > 0) // if there are no posts, update the before and after
                    {
                        subreddit.Before = $"t3_{subreddit.Posts.FirstOrDefault().Id}" ?? string.Empty;
                        subreddit.After = jObject["data"]["after"]?.ToString();

                        if (!string.IsNullOrEmpty(subreddit.Before))
                            _lastBefore = subreddit.Before;
                    }

                    ScheduleNextRequest(_rateLimitRemaining, _rateLimitReset);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    // Wait until the reset period passes before retrying
                    await Task.Delay(_rateLimitReset * 1000);
                    return await GetSubredditAsync(subredditName);
                }

                return subreddit;
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred: {ex.Message}");
            }
            finally
            {
                response?.Dispose();
            }
        }

        /// <summary>
        /// Schedule the next request based on the rate limit headers
        /// </summary>
        /// <param name="rateLimitRemaining"></param>
        /// <param name="rateLimitReset"></param>
        public void ScheduleNextRequest(int rateLimitRemaining, int rateLimitReset)
        {
            if (_requestTimer == null)
            {   //create a new timer
                _requestTimer = new System.Timers.Timer();
                _requestTimer.Elapsed += OnTimedEvent;
            }

            // Calculate the delay for the next request
            double delay;
            if (rateLimitRemaining > 0)
            {
                // Spread out the remaining requests evenly over the reset period
                delay = (rateLimitReset / (double)rateLimitRemaining) * 1000;
            }
            else
            {
                // no remaining time left, wait for the reset period
                delay = rateLimitReset * 1000;
            }

            if (delay == 0)
                delay = 1000;  // delay of 1 second

            _requestTimer.Interval = delay;
            _requestTimer.Start();
        }

        private void OnTimedEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            _requestTimer.Stop();

            // Raise the event
            OnSubredditUpdated(_lastBefore);
        }

        protected virtual void OnSubredditUpdated(string lastBefore)
        {
            // Raise the event back to the client
            SubredditUpdated?.Invoke(this, new SubredditEventArgs { Before = lastBefore });
        }
    }
}