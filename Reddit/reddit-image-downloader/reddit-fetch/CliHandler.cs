using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace reddit_fetch
{
    public static class CliHandler
    {
        public static AppConfig Config { get; set; } = null!;

        public static Task<int> HandleAsync(string[] args)
        {
            // Manual CLI parsing to avoid System.CommandLine version/API mismatches.
            // Commands:
            //   download
            //   list
            //   add-subreddit <name>
            //   remove-subreddit <name>

            if (args == null || args.Length == 0)
            {
                PrintHelp();
                return Task.FromResult(1);
            }

            var cmd = (args[0] ?? string.Empty).Trim().ToLowerInvariant();
            var rest = args.Skip(1).ToArray();

            try
            {
                switch (cmd)
                {
                    case "download":
                        return RunDownloadAsync();

                    case "list":
                        ListHandler();
                        return Task.FromResult(0);

                    case "add-subreddit":
                    case "add":
                        if (rest.Length < 1)
                        {
                            Logger.LogError("add-subreddit requires a subreddit name.");
                            PrintHelp();
                            return Task.FromResult(1);
                        }
                        AddSubredditHandler(rest[0]);
                        return Task.FromResult(0);

                    case "remove-subreddit":
                    case "remove":
                    case "rm":
                        if (rest.Length < 1)
                        {
                            Logger.LogError("remove-subreddit requires a subreddit name.");
                            PrintHelp();
                            return Task.FromResult(1);
                        }
                        RemoveSubredditHandler(rest[0]);
                        return Task.FromResult(0);

                    case "-h":
                    case "--help":
                    case "help":
                    case "/?":
                        PrintHelp();
                        return Task.FromResult(0);

                    default:
                        Logger.LogError($"Unknown command '{cmd}'.");
                        PrintHelp();
                        return Task.FromResult(1);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"CLI error: {ex.Message}");
                return Task.FromResult(1);
            }
        }

        private static async Task<int> RunDownloadAsync()
        {
            await DownloadHandler();
            return 0;
        }

        private static void PrintHelp()
        {
            Console.WriteLine(
@"reddit-fetch CLI

Usage:
  reddit-fetch download
      Download new posts from configured subreddits.

  reddit-fetch list
      List configured subreddits and their states.

  reddit-fetch add-subreddit <name>
      Add a subreddit to the config (without r/).

  reddit-fetch remove-subreddit <name>
      Remove a subreddit from the config.

Notes:
  - Commands are case-insensitive.
  - You can also use aliases:
      add-subreddit -> add
      remove-subreddit -> remove | rm
");
        }

        public static async Task DownloadHandler()
        {
            Logger.LogInfo("Starting download operation...");

            if (Config?.Subreddits == null || Config.Subreddits.Count == 0)
            {
                Logger.LogError("No subreddits listed.");
                return;
            }

            foreach (var subreddit in Config.Subreddits)
            {
                if (!subreddit.IsValid())
                {
                    Logger.LogError($"Invalid subreddit name '{subreddit.Name}'. Skipping.");
                    subreddit.LastCheckDate = DateTime.UtcNow;
                    continue;
                }

                Logger.LogInfo($"Fetching posts for r/{subreddit.Name} since {subreddit.LastPostDate:u} ...");

                string newPosts;
                try
                {
                    newPosts = await RedditApiHelper.GetNewPostsAsync(subreddit.Name, subreddit.LastPostDate);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to fetch posts for subreddit '{subreddit.Name}': {ex.Message}");
                    subreddit.LastCheckDate = DateTime.UtcNow;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(newPosts))
                {
                    Logger.LogInfo($"No data returned for subreddit '{subreddit.Name}'. Skipping.");
                    subreddit.LastCheckDate = DateTime.UtcNow;
                    continue;
                }

                RedditListing redditListing;
                try
                {
                    redditListing = System.Text.Json.JsonSerializer.Deserialize<RedditListing>(newPosts);
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to parse JSON for subreddit '{subreddit.Name}': {ex.Message}");
                    subreddit.LastCheckDate = DateTime.UtcNow;
                    continue;
                }

                if (redditListing?.Data?.Children == null || redditListing.Data.Children.Count == 0)
                {
                    Logger.LogInfo($"No new posts found for subreddit '{subreddit.Name}'.");
                    subreddit.LastCheckDate = DateTime.UtcNow;
                    continue;
                }

                DateTime mostRecentPostDate = subreddit.LastPostDate;

                foreach (var child in redditListing.Data.Children)
                {
                    var post = child.Data;
                    if (post == null)
                    {
                        Logger.LogInfo("Skipping a child with no post data.");
                        continue;
                    }

                    if (post.Url == null)
                    {
                        Logger.LogInfo($"Skipping post '{post.Title}' because URL is null.");
                        continue;
                    }

                    DateTime postDateUtc = DateTimeOffset.FromUnixTimeSeconds((long)post.CreatedUtc).UtcDateTime;

                    if (postDateUtc <= subreddit.LastPostDate)
                        continue; // already processed

                    bool success = await RedditApiHelper.DownloadAndProcessPostAsync(post);

                    if (success)
                    {
                        Logger.LogInfo($"Successfully downloaded post '{post.Title}'.");

                        if (postDateUtc > mostRecentPostDate)
                        {
                            mostRecentPostDate = postDateUtc;
                        }
                    }
                }

                subreddit.LastPostDate = mostRecentPostDate;
                subreddit.LastCheckDate = DateTime.UtcNow;
            }

            Config.Save();
        }

        public static void ListHandler()
        {
            if (Config == null || Config.Subreddits == null || Config.Subreddits.Count == 0)
            {
                Console.WriteLine("No subreddits configured.");
                return;
            }

            Console.WriteLine("Configured subreddits:");
            foreach (var sr in Config.Subreddits)
            {
                Console.WriteLine($"- r/{sr.Name} | LastCheck: {sr.LastCheckDate:u} | LastPost: {sr.LastPostDate:u}");
            }
        }

        public static void AddSubredditHandler(string name)
        {
            if (Config?.Subreddits == null)
                Config!.Subreddits = new List<SubredditInfo>();

            var trimmed = (name ?? string.Empty).Trim();
            if (trimmed.StartsWith("r/", StringComparison.OrdinalIgnoreCase))
                trimmed = trimmed.Substring(2);

            var sr = new SubredditInfo { Name = trimmed };

            if (!sr.IsValid())
            {
                Logger.LogError($"Invalid subreddit name '{name}'.");
                return;
            }

            if (Config.Subreddits.Any(s => string.Equals(s.Name, sr.Name, StringComparison.OrdinalIgnoreCase)))
            {
                Logger.LogError($"Subreddit '{sr.Name}' is already configured.");
                return;
            }

            Config.Subreddits.Add(sr);
            Config.Save();
            Logger.LogInfo($"Added subreddit '{sr.Name}'.");
        }

        public static void RemoveSubredditHandler(string name)
        {
            if (Config?.Subreddits == null || Config.Subreddits.Count == 0)
            {
                Logger.LogError("No subreddits configured.");
                return;
            }

            var trimmed = (name ?? string.Empty).Trim();
            if (trimmed.StartsWith("r/", StringComparison.OrdinalIgnoreCase))
                trimmed = trimmed.Substring(2);

            var match = Config.Subreddits.FirstOrDefault(s =>
                string.Equals(s.Name, trimmed, StringComparison.OrdinalIgnoreCase));

            if (match == null)
            {
                Logger.LogError($"Subreddit '{name}' not found.");
                return;
            }

            Config.Subreddits.Remove(match);
            Config.Save();
            Logger.LogInfo($"Removed subreddit '{match.Name}'.");
        }
    }
}

