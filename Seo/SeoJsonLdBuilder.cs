using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace ToolApp.Seo;

public static class SeoJsonLdBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static string Build(
        string baseUrl,
        string canonicalUrl,
        string documentTitle,
        string metaDesc,
        bool isZh,
        ToolSeoEntry? entry)
    {
        var graph = new List<object>
        {
            new Dictionary<string, object>
            {
                ["@type"] = "WebSite",
                ["@id"] = $"{baseUrl}/#website",
                ["name"] = "ToolApp",
                ["url"] = baseUrl,
                ["description"] = isZh
                    ? "免费在线工具箱：图片、文本、编码、转换与开发对照表，支持中英文。"
                    : "Free online toolbox for images, text, encoding, converters, and developer cheat sheets. English and Chinese.",
                ["inLanguage"] = new[] { "en", "zh-CN" },
                ["publisher"] = new Dictionary<string, object> { ["@id"] = $"{baseUrl}/#organization" }
            },
            new Dictionary<string, object>
            {
                ["@type"] = "Organization",
                ["@id"] = $"{baseUrl}/#organization",
                ["name"] = "ToolApp",
                ["url"] = baseUrl
            }
        };

        if (entry == null || entry.Path == "/")
        {
            graph.Add(new Dictionary<string, object>
            {
                ["@type"] = "WebApplication",
                ["@id"] = $"{baseUrl}/#webapp",
                ["name"] = "ToolApp",
                ["url"] = baseUrl,
                ["description"] = metaDesc,
                ["applicationCategory"] = "UtilitiesApplication",
                ["operatingSystem"] = "Any",
                ["browserRequirements"] = "Requires JavaScript. Many tools run in the browser.",
                ["inLanguage"] = new[] { "en", "zh-CN" },
                ["offers"] = Offer()
            });
        }
        else
        {
            var pageId = $"{canonicalUrl}#webpage";
            graph.Add(new Dictionary<string, object>
            {
                ["@type"] = "WebPage",
                ["@id"] = pageId,
                ["url"] = canonicalUrl,
                ["name"] = documentTitle,
                ["description"] = metaDesc,
                ["isPartOf"] = new Dictionary<string, object> { ["@id"] = $"{baseUrl}/#website" },
                ["inLanguage"] = isZh ? "zh-CN" : "en",
                ["about"] = new Dictionary<string, object> { ["@id"] = $"{canonicalUrl}#app" }
            });

            graph.Add(new Dictionary<string, object>
            {
                ["@type"] = "BreadcrumbList",
                ["itemListElement"] = new object[]
                {
                    new Dictionary<string, object>
                    {
                        ["@type"] = "ListItem",
                        ["position"] = 1,
                        ["name"] = isZh ? "首页" : "Home",
                        ["item"] = baseUrl + "/"
                    },
                    new Dictionary<string, object>
                    {
                        ["@type"] = "ListItem",
                        ["position"] = 2,
                        ["name"] = entry.Category(isZh),
                        ["item"] = baseUrl + "/"
                    },
                    new Dictionary<string, object>
                    {
                        ["@type"] = "ListItem",
                        ["position"] = 3,
                        ["name"] = entry.Title(isZh),
                        ["item"] = canonicalUrl
                    }
                }
            });

            graph.Add(new Dictionary<string, object>
            {
                ["@type"] = "SoftwareApplication",
                ["@id"] = $"{canonicalUrl}#app",
                ["name"] = entry.Title(isZh) + " - ToolApp",
                ["url"] = canonicalUrl,
                ["description"] = metaDesc,
                ["applicationCategory"] = entry.ApplicationCategory,
                ["operatingSystem"] = "Any",
                ["browserRequirements"] = "Requires a modern browser with JavaScript.",
                ["inLanguage"] = new[] { "en", "zh-CN" },
                ["isPartOf"] = new Dictionary<string, object> { ["@id"] = $"{baseUrl}/#website" },
                ["offers"] = Offer()
            });

            if (entry.Faqs.Count > 0)
            {
                graph.Add(new Dictionary<string, object>
                {
                    ["@type"] = "FAQPage",
                    ["@id"] = $"{canonicalUrl}#faq",
                    ["mainEntity"] = entry.Faqs.Select(f => new Dictionary<string, object>
                    {
                        ["@type"] = "Question",
                        ["name"] = f.Question(isZh),
                        ["acceptedAnswer"] = new Dictionary<string, object>
                        {
                            ["@type"] = "Answer",
                            ["text"] = f.Answer(isZh)
                        }
                    }).ToArray()
                });
            }

            var steps = entry.HowToSteps(isZh);
            if (steps.Count > 0)
            {
                var howTo = new Dictionary<string, object>
                {
                    ["@type"] = "HowTo",
                    ["@id"] = $"{canonicalUrl}#howto",
                    ["name"] = isZh
                        ? $"如何使用{entry.TitleZh}"
                        : $"How to use {entry.TitleEn}",
                    ["description"] = metaDesc,
                    ["step"] = steps.Select((s, i) => new Dictionary<string, object>
                    {
                        ["@type"] = "HowToStep",
                        ["position"] = i + 1,
                        ["name"] = s,
                        ["text"] = s
                    }).ToArray()
                };
                graph.Add(howTo);
            }

            if (entry.Related != null && entry.Related.Count > 0)
            {
                graph.Add(new Dictionary<string, object>
                {
                    ["@type"] = "ItemList",
                    ["@id"] = $"{canonicalUrl}#related",
                    ["name"] = isZh ? "相关工具" : "Related tools",
                    ["itemListElement"] = entry.Related.Select((r, i) => new Dictionary<string, object>
                    {
                        ["@type"] = "ListItem",
                        ["position"] = i + 1,
                        ["name"] = r.Title(isZh),
                        ["url"] = baseUrl + (r.Path.StartsWith("/") ? r.Path : "/" + r.Path)
                    }).ToArray()
                });
            }
        }

        var payload = new Dictionary<string, object>
        {
            ["@context"] = "https://schema.org",
            ["@graph"] = graph.ToArray()
        };

        return JsonSerializer.Serialize(payload, JsonOptions);
    }

    private static Dictionary<string, object> Offer() => new()
    {
        ["@type"] = "Offer",
        ["price"] = "0",
        ["priceCurrency"] = "USD"
    };
}
