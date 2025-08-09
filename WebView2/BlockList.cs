using System.Collections.Generic;

namespace WebView2Browser
{
    public static class BlockList
    {
        public static readonly HashSet<string> AdDomains = new HashSet<string>
        {
            "tsyndicate.com",
            "doubleclick.net",
            "googleads.g.doubleclick.net",
            "adservice.google.com",
            "googlesyndication.com",
            "adnxs.com",
            "amazon-adsystem.com",
            "scorecardresearch.com",
            "facebook.com/tr/",
            "ads.twitter.com",
            "advertising.com",
            "taboola.com",
            "outbrain.com"
        };

        public static readonly HashSet<string> AdClasses = new HashSet<string>
        {
            "mn-thumb__holder",
            "mn-thumb__img",
            "ad-container",
            "ad-banner",
            "ad-wrapper",
            "ad-unit",
            "ad-placeholder",
            "ad-slot",
            "advertisement",
            "advert",
            "ad__",
            "adsbygoogle",
            "ad-box",
            "ad-head",
            "ad-body",
            "w",          // top-level wrapper
            "cnts",       // inner flex container
        };

        public static readonly HashSet<string> AdAttributes = new HashSet<string>
        {
            "data-ad",
            "data-ad-client",
            "data-ad-slot",
            "data-ad-targeting",
            "data-adbreak",
            "data-ad-position"
        };
    }
}
