using JH.Service.Reddit.Models;

namespace JH.Service.Reddit
{
    public class SubredditService
    {
        /// <summary>
        /// Method to find the author with the most posts and the count of their posts
        /// </summary>
        /// <returns>Author and Post Count</returns>
        public static (string Author, string PostCount) GetAuthorWithMostPosts(Subreddit subreddit)
        {
            if (subreddit == null || subreddit.Posts == null || !subreddit.Posts.Any())
            {
                return ("0", "0");
            }

            var mostPosts = subreddit.Posts
                .GroupBy(p => p.Author)
                .OrderByDescending(gp => gp.Count())
                .FirstOrDefault();

            string author = mostPosts?.Key ?? "";
            string userWithMostPostsCount = mostPosts?.Count().ToString() ?? "0";

            return (author, userWithMostPostsCount);
        }

        /// <summary>
        /// Method to find the author and score of the most upvoted post
        /// </summary>
        /// <returns>Author and Score</returns>
        public static (string Author, int Score) GetAuthorWithMostUpvotedPost(Subreddit subreddit)
        {
            if (subreddit == null || subreddit.Posts == null || !subreddit.Posts.Any())
            {
                return ("No Author", 0);
            }

            var mostUpvotedPost = subreddit.Posts
                .OrderByDescending(p => p.Score)
                .FirstOrDefault();

            if (mostUpvotedPost == null)
            {
                return ("", 0);
            }

            return (mostUpvotedPost.Author, mostUpvotedPost.Score);
        }
    }
}