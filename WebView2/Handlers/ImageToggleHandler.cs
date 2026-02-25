
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebView2Browser
{
    public class ImageToggleHandler
    {
        private readonly CoreWebView2 _webView;
        private bool _imagesDisabled = true; // Default to blocked
        private bool _videosDisabled = true; // Default to blocked
        private bool _backgroundImagesDisabled = true;

        public ImageToggleHandler(CoreWebView2 webView)
        {
            _webView = webView ?? throw new ArgumentNullException(nameof(webView));
            InitializeBlocking();
            SetupNavigationHandlers();
        }

        public void ToggleImages()
        {
            _imagesDisabled = !_imagesDisabled;
        }

        public void ToggleVideos()
        {
            _videosDisabled = !_videosDisabled;
        }

        public void ToggleResourceLoading(bool disableImages, bool disableVideos)
        {
            _imagesDisabled = disableImages;
            _videosDisabled = disableVideos;
            _webView.Reload();
        }

        private void InitializeBlocking()
        {
            _webView.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Image);
            _webView.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Media);
            _webView.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Other);
            _webView.WebResourceRequested += OnWebResourceRequested;
        }

        private void OnWebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (_imagesDisabled && e.ResourceContext == CoreWebView2WebResourceContext.Image)
            {
                e.Response = _webView.Environment.CreateWebResourceResponse(null, 403, "Forbidden", "Image blocked by browser settings");
                return;
            }

            if (_videosDisabled && e.ResourceContext == CoreWebView2WebResourceContext.Media)
            {
                e.Response = _webView.Environment.CreateWebResourceResponse(null, 403, "Forbidden", "Media blocked by browser settings");
                return;
            }

            if (_backgroundImagesDisabled && 
                (e.ResourceContext == CoreWebView2WebResourceContext.Image || e.ResourceContext == CoreWebView2WebResourceContext.Other))
            {
                string requestUri = e.Request.Uri;
                if (requestUri.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
                {
                    e.Response = _webView.Environment.CreateWebResourceResponse(null, 403, "Forbidden", "Blob background image blocked");
                    return;
                }
            }
        }

        private void SetupNavigationHandlers()
        {
            _webView.AddScriptToExecuteOnDocumentCreatedAsync(
                "(() => { document.querySelectorAll('img').forEach(img => { if (!img.src.startsWith('data:')) img.style.display = 'none'; }); })();");
        }
    }
}
