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
        private bool _backgroundImagesDisabled = true; // Flag to control background image blocking logic

        public ImageToggleHandler(CoreWebView2 webView)
        {
            _webView = webView ?? throw new ArgumentNullException(nameof(webView));
            InitializeBlocking();
            SetupNavigationHandlers();
        }

        public void ToggleImages()
        {
            _imagesDisabled = !_imagesDisabled;
            // Note: Toggling might require navigation reload to take full effect on already loaded resources
        }

        public void ToggleVideos()
        {
            _videosDisabled = !_videosDisabled;
            // Note: Toggling might require navigation reload to take full effect on already loaded resources
        }

        private void InitializeBlocking()
        {
            // Add filters for standard image and media requests
            _webView.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Image);
            _webView.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Media);

            // Add filter for 'Other' context. This is crucial for catching:
            // - blob: URLs (like the one in your Bing example)
            // - Dynamically generated images
            // - Some SVGs or resources not strictly classified as Image
            _webView.AddWebResourceRequestedFilter("*", CoreWebView2WebResourceContext.Other);

            _webView.WebResourceRequested += OnWebResourceRequested;
        }

        private void OnWebResourceRequested(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            // Block standard images if disabled
            if (_imagesDisabled && e.ResourceContext == CoreWebView2WebResourceContext.Image)
            {
                e.Response = _webView.Environment.CreateWebResourceResponse(null, 403, "Forbidden", "Image blocked by browser settings");
                return;
            }

            // Block videos if disabled
            if (_videosDisabled && e.ResourceContext == CoreWebView2WebResourceContext.Media)
            {
                e.Response = _webView.Environment.CreateWebResourceResponse(null, 403, "Forbidden", "Media blocked by browser settings");
                return;
            }

            // Attempt to block background images
            // Check if background image blocking logic is conceptually enabled
            // and the resource is an Image or Other (for blobs)
            if (_backgroundImagesDisabled &&
                (e.ResourceContext == CoreWebView2WebResourceContext.Image ||
                 e.ResourceContext == CoreWebView2WebResourceContext.Other))
            {
                string requestUri = e.Request.Uri;

                // Heuristic 1: Block blob: URLs - These are often used for dynamically loaded backgrounds
                if (requestUri.StartsWith("blob:", StringComparison.OrdinalIgnoreCase))
                {
                    // Optionally, log for debugging: Console.WriteLine($"Blocking blob URL: {requestUri}");
                    e.Response = _webView.Environment.CreateWebResourceResponse(null, 403, "Forbidden", "Blob background image blocked");
                    return;
                }

                // Heuristic 2: Block based on common background image patterns (if identifiable)
                // This is tricky without knowing the site structure, but you can add checks here
                // Example (uncomment and adapt if needed):
                /*
                var lowerUri = requestUri.ToLowerInvariant();
                if (lowerUri.Contains("/background") || lowerUri.Contains("/bg_") || lowerUri.Contains("wallpaper"))
                {
                     e.Response = _webView.Environment.CreateWebResourceResponse(null, 403, "Forbidden", "Potential background image blocked");
                     return;
                }
                */

                // Heuristic 3: Block based on known ad domains (leveraging existing AdBlocker logic might be better,
                // but for self-contained ImageToggleHandler, you could duplicate or pass a reference)
                // This requires access to BlockList.AdDomains or similar.
                // Example placeholder (requires BlockList.AdDomains to be accessible or passed in):
                /*
                if (BlockList.AdDomains.Any(domain => requestUri.IndexOf(domain, StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    e.Response = _webView.Environment.CreateWebResourceResponse(null, 403, "Forbidden", "Image from ad domain blocked");
                    return;
                }
                */

                // Add more heuristics here if needed...
            }
        }


        private void SetupNavigationHandlers()
        {
            // 1️⃣  Inject CSS to hide standard image tags and common background image classes/elements
            _webView.AddScriptToExecuteOnDocumentCreatedAsync(
                """
        (() => {
            /* Create a style element to hold our blocking rules */
            const style = document.createElement('style');
            style.id = 'webview2-bg-blocker-id-specific'; // Give it an ID
            style.textContent = `
                /* === Existing Rules for <img>, <svg>, <video>, <iframe>, and inline SVG backgrounds === */
                img, svg, picture, image { display: none !important; }
                .tip svg, .w svg, .popup svg { display: none !important; }
                *[style*="background-image:url('data:image/svg+xml"],
                *[style*="background-image:url(\\"image/svg+xml"],
                *[style*="background-image:url(data:image/svg+xml"]
                { display: none !important; background-image: none !important; }
                video, iframe { display: none !important; }

                /* === NEW/ENHANCED RULES for Background Images === */
                /* --- Target the specific ID and Class from the Bing HTML --- */
                #img_cont, .img_cont, .hp_top_cover {
                    /* Attempt to override background-image via CSS */
                    background-image: none !important;
                    /* Optionally, hide the entire element if override isn't enough */
                    /* display: none !important; */
                }
                /* --- Target shaders container --- */
                .shaders {
                     background-image: none !important;
                     /* Hiding shaders might be necessary */
                     /* display: none !important; */
                }
                /* --- Target specific Bing icon classes if needed --- */
                /* Add .hbic_* classes here if they use background images via CSS */
            `;
            document.documentElement.firstElementChild.appendChild(style);
        })();
        """
            );

            // 2️⃣  Use NavigationCompleted to run a script that specifically targets the element
            // and removes/overrides the inline style more directly and repeatedly.
            _webView.NavigationCompleted += async (_, args) =>
            {
                if (!args.IsSuccess) return;

                await _webView.ExecuteScriptAsync(
                    """
            (() => {
                const hideImagesAndBackgrounds = () => {
                    // Hide standard image/svg tags (existing logic)
                    document.querySelectorAll(
                        'img,svg,picture,image,.tip svg,.w svg,.popup svg'
                    ).forEach(el => el.style.display = 'none');

                    // --- NEW/ENHANCED: Directly target the specific Bing element ---
                    const imgContElement = document.getElementById('img_cont');
                    if (imgContElement) {
                        // Remove the background-image property from the inline style entirely
                        imgContElement.style.removeProperty('background-image');
                        // Also force it to none via style attribute
                        imgContElement.style.backgroundImage = 'none !important';
                        // Optionally, hide the element completely if needed
                        // imgContElement.style.display = 'none !important';
                    }

                    const hpTopCoverElement = document.getElementById('hp_top_cover');
                    if (hpTopCoverElement) {
                        hpTopCoverElement.style.removeProperty('background-image');
                        hpTopCoverElement.style.backgroundImage = 'none !important';
                        // Optionally, hide the element completely if needed
                        // hpTopCoverElement.style.display = 'none !important';
                    }

                    // Target elements by class name and remove inline background styles
                    const elementsToClear = document.querySelectorAll('.img_cont, .hp_top_cover, .shaders');
                    elementsToClear.forEach(el => {
                        // Remove the property from the inline style
                        el.style.removeProperty('background-image');
                        // Forcefully set it to none
                        el.style.setProperty('background-image', 'none', 'important');
                        // Optionally, hide the element completely if needed
                        // el.style.setProperty('display', 'none', 'important');
                    });

                    // Optional: Apply a broad override again after DOM changes (use cautiously)
                    // Uncomment the lines below if needed.
                    /*
                    document.querySelectorAll('*').forEach(el => {
                        // Only apply if it seems to have a background image set
                        // if (el.style.backgroundImage && el.style.backgroundImage !== 'none') {
                            el.style.setProperty('background-image', 'none', 'important');
                        // }
                    });
                    */
                };

                // Run the hiding logic immediately
                hideImagesAndBackgrounds();

                // Keep watching for late inserts/mutations and re-run the logic
                // This is crucial for elements that are added or modified dynamically
                const observer = new MutationObserver(hideImagesAndBackgrounds);
                observer.observe(document.body, { childList: true, subtree: true });

                // Optional: Run the logic periodically as a last resort
                // This can help catch changes that MutationObserver might miss
                // or if styles are reapplied very quickly by page scripts.
                // Adjust interval time (ms) as needed, consider performance.
                // const intervalId = setInterval(hideImagesAndBackgrounds, 500); // Every 500ms
                // Clear interval on page unload/navigation start if needed (advanced)
            })();
            """
                );
            };
        }

        // Optional: Method to toggle background image blocking if you want a separate control
        // public void ToggleBackgroundImages()
        // {
        //     _backgroundImagesDisabled = !_backgroundImagesDisabled;
        //     // Note: Toggling might require navigation reload to take full effect
        // }
    }
}
