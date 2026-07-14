namespace ToolApp
{
    public class SeoOptions
    {
        /// <summary>
        /// Production site URL, e.g. https://example.com (no trailing slash).
        /// If empty, canonical and sitemap use the current request host.
        /// </summary>
        public string PublicBaseUrl { get; set; } = string.Empty;

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
