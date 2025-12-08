using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace reddit_fetch
{
    /// <summary>
    /// Provides methods for interacting with the Reddit API.
    /// </summary>
    public static class RedditApiHelper
    {
        public static AppConfig Config { get; set; } = null!;

        private static readonly HttpClient HttpClient = new HttpClient();
        private static string? _accessToken;
        private static DateTime _tokenExpiryUtc;

        /// <summary>
        /// Adds headers in a version/format tolerant way.
        /// </summary>
        private static void ApplyCommonHeaders(HttpRequestMessage req, bool includeAuth)
        {
            // User-Agent is a "typed" header if you use req.Headers.UserAgent.ParseAdd(...)
            // That path rejects comments like "(by ...)".
            // Use raw header add instead.
            req.Headers.TryAddWithoutValidation("User-Agent", Config.UserAgent);

            if (includeAuth && !string.IsNullOrWhiteSpace(_accessToken))
            {
                req.Headers.Authorization = new AuthenticationHeaderValue("bearer", _accessToken);
            }
        }

        /// <summary>
        /// Ensures we have a valid OAuth token.
        /// </summary>
        private static async Task EnsureTokenAsync()
        {
            if (!string.IsNullOrWhiteSpace(_accessToken) && DateTime.UtcNow < _tokenExpiryUtc)
                return;

            // Installed app flow
            var form = new FormUrlEncodedContent(new[]
    {
        new KeyValuePair<string,string>("grant_type", "https://oauth.reddit.com/grants/installed_client"),
        new KeyValuePair<string,string>("device_id", "DO_NOT_TRACK_THIS_DEVICE")
    });

            using var req = new HttpRequestMessage(HttpMethod.Post, "https://www.reddit.com/api/v1/access_token");
            req.Headers.TryAddWithoutValidation("User-Agent", Config.UserAgent);
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Config.ClientId}:")));

            var response = await HttpClient.SendAsync(req);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"OAuth error {response.StatusCode}: {json}");

            var token = JsonSerializer.Deserialize<TokenResponse>(json);

            _accessToken = token!.access_token;
            _tokenExpiryUtc = DateTime.UtcNow.AddMinutes(50);
        }

        /// <summary>
        /// Fetches /new posts since a given time.
        /// </summary>
        public static async Task<string> GetNewPostsAsync(string subreddit, DateTime sinceUtc)
        {
            await EnsureTokenAsync();

            var url = $"https://oauth.reddit.com/r/{subreddit}/new?limit=100";
            using var req = new HttpRequestMessage(HttpMethod.Get, url);
            ApplyCommonHeaders(req, includeAuth: true);

            var response = await HttpClient.SendAsync(req);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Downloads a post's media (if image) and runs duplicate filtering.
        /// </summary>
        public static async Task<bool> DownloadAndProcessPostAsync(RedditPost post)
        {
            if (post.Url == null)
                return false;

            if (post.IsVideo || string.Equals(post.PostHint, "rich:video", StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogInfo($"Skipping video post '{post.Title}'.");
                return false;
            }

            var safeTitle = PathHelper.MakeSafeFilename(post.Title ?? "untitled");
            var ext = Path.GetExtension(post.Url);
            if (string.IsNullOrWhiteSpace(ext))
                ext = ".jpg";

            var fileName = $"{safeTitle}_{post.Id}{ext}";
            var targetPath = Path.Combine(Config.DownloadPath, fileName);

            Directory.CreateDirectory(Config.DownloadPath);

            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, post.Url);
                ApplyCommonHeaders(req, includeAuth: false);

                var response = await HttpClient.SendAsync(req);
                response.EnsureSuccessStatusCode();

                await using var fs = new FileStream(targetPath, FileMode.Create, FileAccess.Write);
                await response.Content.CopyToAsync(fs);

                var (ok, reason, hash) = await ImageFilterHelper.ValidateAndHashImageAsync(targetPath);
                if (!ok)
                {
                    Logger.LogInfo($"Rejected '{fileName}': {reason}");
                    File.Delete(targetPath);
                    return false;
                }

                if (HashDatabaseHelper.IsNearDuplicate(targetPath, hash))
                {
                    Logger.LogInfo($"Duplicate '{fileName}' removed.");
                    File.Delete(targetPath);
                    return false;
                }

                HashDatabaseHelper.UpsertHash(targetPath, hash);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Download failed for '{post.Title}': {ex.Message}");
                try { if (File.Exists(targetPath)) File.Delete(targetPath); } catch { }
                return false;
            }
        }

        private sealed class TokenResponse
        {
            public string access_token { get; set; } = string.Empty;
            public string token_type { get; set; } = string.Empty;
            public string scope { get; set; } = string.Empty;
        }
    }
}
