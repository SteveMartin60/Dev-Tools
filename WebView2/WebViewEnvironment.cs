using Microsoft.Web.WebView2.Core;

namespace WebView2Browser
{
    public static class WebViewEnvironment
    {
        private static CoreWebView2Environment _sharedEnvironment;
        private static readonly SemaphoreSlim _envLock = new SemaphoreSlim(1, 1);

        public static async Task<CoreWebView2Environment> GetSharedEnvironmentAsync()
        {
            if (_sharedEnvironment == null)
            {
                await _envLock.WaitAsync();
                try
                {
                    if (_sharedEnvironment == null)
                    {
                        _sharedEnvironment = await CoreWebView2Environment.CreateAsync();
                    }
                }
                finally
                {
                    _envLock.Release();
                }
            }
            return _sharedEnvironment;
        }
    }
}
