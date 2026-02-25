using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;

namespace WebView2Browser.Handlers
{
    public class ImageToggleHandler
    {
        private readonly CoreWebView2 _webView;
        private bool _imagesDisabled = false;
        private bool _videosDisabled = false;

        public ImageToggleHandler(CoreWebView2 webView)
        {
            _webView = webView ?? throw new ArgumentNullException(nameof(webView));
            InitializeBlocking();
        }

        public void ToggleImages()
        {
            _imagesDisabled = !_imagesDisabled;
            ApplyBlocking();
        }

        public void ToggleVideos()
        {
            _videosDisabled = !_videosDisabled;
            ApplyBlocking();
        }

        private void InitializeBlocking()
        {
            _webView.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Image);
            _webView.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Media);
            _webView.WebResourceRequested += OnWebResourceRequested;
            _webView.NavigationCompleted += OnNavigationCompleted;
        }

        private void OnNavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess) ApplyBlocking();
        }

        private async void ApplyBlocking()
        {
            string script = $$"""
(() => {
    const imagesDisabled = {{_imagesDisabled.ToString().ToLower()}};
    const videosDisabled = {{_videosDisabled.ToString().ToLower()}};

    // Persist live state for observer callbacks (avoid stale closure values)
    window.__rtBlockState = window.__rtBlockState || {};
    window.__rtBlockState.imagesDisabled = imagesDisabled;
    window.__rtBlockState.videosDisabled = videosDisabled;

    // Ensure style element exists (single instance)
    const styleId = "__rtBlockStyle";
    let styleEl = document.getElementById(styleId);

    if (!styleEl)
    {
        styleEl = document.createElement("style");
        styleEl.id = styleId;
        (document.head || document.documentElement).appendChild(styleEl);
    }

    // CSS-based hiding for current and future matching elements
    const css = `
        ${imagesDisabled ? `
            img,
            picture,
            source[type^="image/"],
            ytd-thumbnail,
            #thumbnail,
            .yt-img-shadow,
            yt-img-shadow
            {
                display: none !important;
                visibility: hidden !important;
            }

            [style*="background-image"]
            {
                background-image: none !important;
            }
        ` : ""}

        ${videosDisabled ? `
            video,
            audio,
            ytd-player,
            #movie_player,
            .html5-video-player,
            .html5-video-container,
            #player-container,
            ytd-shorts,
            ytd-reel-video-renderer,
            ytd-shorts-player,
            #shorts-container
            {
                display: none !important;
                visibility: hidden !important;
            }
        ` : ""}
    `;

    styleEl.textContent = css;

    function hideSelector(root, selector, hide)
    {
        if (!root || !root.querySelectorAll) return;

        try
        {
            root.querySelectorAll(selector).forEach((el) => {
                if (hide)
                {
                    el.style.setProperty("display", "none", "important");
                    el.style.setProperty("visibility", "hidden", "important");

                    if (el.tagName === "VIDEO" || el.tagName === "AUDIO")
                    {
                        try { el.pause(); } catch {}
                        try { el.removeAttribute("src"); } catch {}
                        try { if (typeof el.load === "function") el.load(); } catch {}
                    }
                }
                else
                {
                    el.style.removeProperty("display");
                    el.style.removeProperty("visibility");
                }
            });
        }
        catch {}
    }

    function applyNow()
    {
        const state = window.__rtBlockState || {};
        const imgOff = !!state.imagesDisabled;
        const vidOff = !!state.videosDisabled;

        hideSelector(document, "img, picture, source[type^='image/'], ytd-thumbnail, #thumbnail, .yt-img-shadow, yt-img-shadow", imgOff);
        hideSelector(document, "video, audio, ytd-player, #movie_player, .html5-video-player, .html5-video-container, #player-container", vidOff);

        // YouTube-specific: pause/remove source from active player media if present
        if (vidOff)
        {
            try
            {
                document.querySelectorAll("video, audio").forEach((m) => {
                    try { m.pause(); } catch {}
                    try { m.removeAttribute("src"); } catch {}
                    try { if (typeof m.load === "function") m.load(); } catch {}
                });
            }
            catch {}
        }
    }

    // Immediate pass
    applyNow();

    // Install observer once; callback reads live state from window.__rtBlockState
    if (!window.__rtBlockObserver)
    {
        window.__rtBlockObserver = new MutationObserver(() => {
            applyNow();
        });

        const startObserve = () => {
            const target = document.body || document.documentElement;
            if (!target) return false;

            try
            {
                window.__rtBlockObserver.observe(target, { childList: true, subtree: true, attributes: false });
                return true;
            }
            catch
            {
                return false;
            }
        };

        if (!startObserve())
        {
            // Retry after DOM settles (important for SPA/page transitions)
            setTimeout(startObserve, 250);
            setTimeout(startObserve, 1000);
        }
    }

    // Optional fetch/XHR blocking for common media endpoints (best-effort only)
    if (!window.__rtNetworkPatchApplied)
    {
        window.__rtNetworkPatchApplied = true;

        const shouldBlockMediaUrl = (url) => {
            const s = String(url || "").toLowerCase();
            if (!s) return false;

            const state = window.__rtBlockState || {};
            if (!state.videosDisabled) return false;

            return s.includes("googlevideo.com") ||
                   s.includes("/videoplayback") ||
                   s.includes(".m3u8") ||
                   s.includes(".mpd");
        };

        try
        {
            const originalFetch = window.fetch;
            if (typeof originalFetch === "function")
            {
                window.fetch = function (...args)
                {
                    const req = args[0];
                    const url = (req && req.url) ? req.url : req;

                    if (shouldBlockMediaUrl(url))
                    {
                        return Promise.resolve(new Response("", { status: 403, statusText: "Blocked" }));
                    }

                    return originalFetch.apply(this, args);
                };
            }
        }
        catch {}

        try
        {
            const originalOpen = XMLHttpRequest.prototype.open;
            XMLHttpRequest.prototype.open = function (method, url, ...rest)
            {
                if (shouldBlockMediaUrl(url))
                {
                    try { this.abort(); } catch {}
                    return;
                }

                return originalOpen.call(this, method, url, ...rest);
            };
        }
        catch {}
    }
})();
""";

            try
            {
                await _webView.ExecuteScriptAsync(script);
            }
            catch
            {
                // Ignore script injection failures on unsupported/navigation-transition states
            }
        }
        private void OnWebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            if (_imagesDisabled && e.ResourceContext == CoreWebView2WebResourceContext.Image)
            {
                e.Response = _webView.Environment.CreateWebResourceResponse(
                    null,
                    403,
                    "Forbidden",
                    "Image blocked");

                return;
            }

            if (_videosDisabled && e.ResourceContext == CoreWebView2WebResourceContext.Media)
            {
                e.Response = _webView.Environment.CreateWebResourceResponse(
                    null,
                    403,
                    "Forbidden",
                    "Media blocked");

                return;
            }

            if (_videosDisabled)
            {
                string uri = e.Request?.Uri ?? string.Empty;

                // Do not gate this behind youtube.com/youtu.be only.
                // Actual video stream traffic commonly comes from googlevideo.com.
                if (uri.Contains("googlevideo.com", StringComparison.OrdinalIgnoreCase) ||
                    uri.Contains("/videoplayback", StringComparison.OrdinalIgnoreCase) ||
                    uri.Contains(".m3u8", StringComparison.OrdinalIgnoreCase) ||
                    uri.Contains(".mpd", StringComparison.OrdinalIgnoreCase))
                {
                    e.Response = _webView.Environment.CreateWebResourceResponse(
                        null,
                        403,
                        "Forbidden",
                        "Video stream blocked");

                    return;
                }
            }
        }
    }
}