using JH.Service.Reddit.Models;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace JH.Service.Reddit
{
    /// <summary>
    /// // I initially thought we had to authenticate and acquire a token.  I left this here to show my initial work
    /// </summary>
    public class AuthenticatedRedditService
    {
        public static async Task<RedditResponse?> GetAccessTokenAsync(string clientId, string clientSecret)
        {
            using HttpClient client = new();

            using HttpRequestMessage request = new()
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://www.reddit.com/api/v1/access_token")
            };

            string auth = Convert.ToBase64String(Encoding.Default.GetBytes($"{clientId}:{clientSecret}"));
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "username", "{provide a username}" },
                    { "password", "{provide a pw}" }
                });

            client.DefaultRequestHeaders.Add("User-Agent", "JHPostStats/0.1 by ToddEvansHome");
            using HttpResponseMessage response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            RedditResponse redditTokenResponse = await response.Content.ReadFromJsonAsync<RedditResponse>();

            return redditTokenResponse;
        }

        // an example of how to use the token to get the user's info
        public static async Task GetMeAsync(string token)
        {
            using HttpClient client = new();

            using HttpRequestMessage request = new()
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri("https://oauth.reddit.com/api/v1/me")
            };

            client.DefaultRequestHeaders.Add("User-Agent", "JHPostStats/0.1 by ToddEvansHome");
            client.DefaultRequestHeaders.Add("Authorization", $"bearer {token}");

            using HttpResponseMessage response = await client.SendAsync(request);

            response.EnsureSuccessStatusCode();

            //This was used to access the response content
            //RedditFeaturesDTO redditFeaturesDTO = await response.Content.ReadFromJsonAsync<RedditFeaturesDTO>();

            string result = await response.Content.ReadAsStringAsync();
        }
    }
}