using JH.Service.Reddit.Models;

namespace JH.Service.Reddit
{
    public interface IRedditService
    {
        Task<Subreddit> GetSubredditAsync(string subredditName, string before = null, int limit = 100);

        void ScheduleNextRequest(int rateLimitRemaining, int rateLimitReset);
    }
}