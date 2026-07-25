namespace ToolApp
{
    public class SeoOptions
    {
        /// <summary>
        /// Canonical public site URL (no trailing slash), e.g. https://toolapp.org.
        /// Used by robots.txt, sitemap.xml, Open Graph and JSON-LD.
        /// Do not leave empty in production — otherwise loopback/proxy hostnames leak into sitemap.
        /// </summary>
        public string PublicBaseUrl { get; set; } = "https://toolapp.org";

        /// <summary>
        /// Fallback meta description when a page does not set ViewData["MetaDescription"].
        /// </summary>
        public string DefaultMetaDescription { get; set; } =
            "Free online tools for images, text, encoding, and conversion. Reference tables, OCR, QR codes, and developer utilities. English and Chinese.";

        /// <summary>
        /// Optional default Open Graph / Twitter image path (e.g. /images/og.png). Absolute URLs are allowed.
        /// </summary>
        public string DefaultOgImagePath { get; set; } = string.Empty;

        /// <summary>
        /// Optional extra paths merged into sitemap.xml (leading slash).
        /// Primary paths come from <see cref="ToolApp.Seo.ToolSeoCatalog"/>.
        /// </summary>
        public string[] SitemapPaths { get; set; } = System.Array.Empty<string>();
    }
}
