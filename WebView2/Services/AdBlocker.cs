// AdBlocker.cs
// Full, un-abbreviated drop-in file
// -------------------------------------------------
// Automatically removes:
//   • Known ad domains
//   • Elements with known ad-related class names
//   • Ad background images
//   • Ad iframes
//   • Social spam pop-ups delivered via iframes / inline HTML

using Microsoft.Web.WebView2.Core;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebView2Browser
{
    public class AdBlocker
    {
        private readonly CoreWebView2 _webView;

        public AdBlocker(CoreWebView2 webView)
        {
            _webView = webView;
            SetupNavigationHandler();
        }

        private void SetupNavigationHandler()
        {
            // 1. Cancel navigation to known ad domains
            _webView.NavigationStarting += (sender, args) =>
            {
                if (BlockList.AdDomains.Any(domain => args.Uri.Contains(domain)))
                {
                    args.Cancel = true;
                }
            };

            // 2. Apply cosmetic blocking after every successful navigation
            _webView.NavigationCompleted += async (sender, args) =>
            {
                if (args.IsSuccess)
                {
                    await ApplyAdBlocking();
                }
            };
        }

        private async Task ApplyAdBlocking()
        {
            // Prepare comma-separated, back-tick-quoted lists
            string classList = string.Join(",", BlockList.AdClasses.Select(c => $"`{c}`"));
            string domainList = string.Join(",", BlockList.AdDomains.Select(d => $"`{d}`"));

            // JavaScript that runs inside every page
            string script = $$"""
                // ---------- 1.  Remove elements linking to known ad domains ----------
                const adDomains = [{{domainList}}];
                adDomains.forEach(domain => {
                    document.querySelectorAll(`a[href*='${domain}']`).forEach(el => el.remove());
                });

                // ---------- 2.  Remove elements with known ad classes ----------
                const adClasses = [{{classList}}];
                adClasses.forEach(className => {
                    const elements = document.getElementsByClassName(className);
                    while (elements.length > 0) elements[0].remove();
                });

                // ---------- 3.  Remove background images served from ad domains ----------
                document.querySelectorAll('[style*="background-image"]').forEach(el => {
                    if (adDomains.some(d => el.style.backgroundImage.includes(d))) el.remove();
                });

                // ---------- 4.  Remove ad iframes ----------
                document.querySelectorAll('iframe').forEach(iframe => {
                    if (adDomains.some(d => iframe.src?.includes(d))) iframe.remove();
                });

                // ---------- 5.  Kill social spam pop-ups ----------
                (() => {
                    // Remove root-level iframes (common spam vector)
                    document.querySelectorAll('body > iframe').forEach(f => {
                        try { f.remove(); } catch {}
                    });

                    // Universal spam detector
                    const killSpam = () => {
                        // a) Remove suspicious iframes (empty src, data:, blob:)
                        document.querySelectorAll('iframe').forEach(f => {
                            if (!f.src || f.src.startsWith('data:') || f.src.startsWith('blob:')) {
                                try {
                                    const doc = f.contentDocument || f.contentWindow?.document;
                                    if (doc && doc.body && doc.body.textContent.includes('Do you want to fuck me?')) {
                                        f.remove();
                                    }
                                } catch { /* cross-origin, ignore */ }
                            }
                        });

                        // b) Remove inline spam nodes
                        document.querySelectorAll('.w.r-b, .w.l-b, .w.r-t, .w.l-t').forEach(el => {
                            if (el.textContent.includes('Do you want to fuck me?')) {
                                el.remove();
                            }
                        });
                    };

                    // Run once immediately
                    killSpam();

                    // Keep watching for late injections
                    new MutationObserver(killSpam).observe(document.body, {
                        childList: true,
                        subtree: true
                    });
                })();
            """;

            await _webView.ExecuteScriptAsync(script);
        }
    }
}
