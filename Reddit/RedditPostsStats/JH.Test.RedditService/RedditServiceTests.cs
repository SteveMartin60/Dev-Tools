using JH.Service.Reddit;
using System.Net;
using System.Text;

namespace JH.Test
{
    public class RedditServiceTests : IDisposable
    {
        private readonly HttpClient _mockHttpClient;
        private readonly RedditService _redditService;

        public RedditServiceTests()
        {
            var handler = new MockHttpMessageHandler();
            _mockHttpClient = new HttpClient(handler);
            _redditService = new RedditService(_mockHttpClient);
        }

        /// <summary>
        /// Mock the message handler
        /// </summary>
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            public virtual HttpResponseMessage Send(HttpRequestMessage request)
            {
                var fakeResponseContent = @"
                    {
                        ""data"": {
                            ""children"": [
                                {
                                    ""data"": {
                                        ""id"": ""post1"",
                                        ""title"": ""Test Post 1"",
                                        ""author"": ""Author1"",
                                        ""score"": 100,
                                        ""thumbnail"": ""http://example.com/thumbnail1.jpg"",
                                        ""url"": ""http://example.com/post1"",
                                        ""num_comments"": 10,
                                        ""created"": 1618886400, // Example Unix timestamp
                                        ""created_utc"": 1618886400
                                    }
                                },
                                {
                                    ""data"": {
                                        ""id"": ""post2"",
                                        ""title"": ""Test Post 2"",
                                        ""author"": ""Author2"",
                                        ""score"": 150,
                                        ""thumbnail"": ""http://example.com/thumbnail2.jpg"",
                                        ""url"": ""http://example.com/post2"",
                                        ""num_comments"": 20,
                                        ""created"": 1618972800,
                                        ""created_utc"": 1618972800
                                    }
                                }
                            ]
                        }
                    }";


                // Simulate a response
                var response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(fakeResponseContent, Encoding.UTF8, "application/json")
                };

                return response;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return Task.FromResult(Send(request));
            }
        }

        [Fact]
        public async Task GetSubredditAsync_ThrowsArgumentException_WhenSubredditNameIsEmpty()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _redditService.GetSubredditAsync(""));
        }

        [Fact]
        public async Task GetSubredditAsync_ReturnsSubreddit_WhenCalledWithValidName()
        {
            string subredditName = "Blazor";
            int limit = 2;
            var subreddit = await _redditService.GetSubredditAsync(subredditName, null, limit);

            Assert.NotNull(subreddit);
            Assert.Equal(subredditName, subreddit.Name);
            Assert.Equal(limit, subreddit.Posts.Count);
        }

        public void Dispose()
        {
            _mockHttpClient?.Dispose();
        }

        // Additional tests for handling errors, rate limiting, and other scenarios...
    }
}