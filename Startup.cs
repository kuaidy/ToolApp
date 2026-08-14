using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.FluentUI.AspNetCore.Components;
using ToolApp.Seo;

namespace ToolApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //???blazor????
            services.AddServerSideBlazor().AddHubOptions(o =>
            {
                o.MaximumReceiveMessageSize = 64 * 1024 * 1024;
            });
            services.AddHttpClient();
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            services.Configure<FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = 22 * 1024 * 1024;
            });
            services.Configure<Microsoft.AspNetCore.Routing.RouteOptions>(options =>
            {
                options.LowercaseUrls = true;
                options.LowercaseQueryStrings = true;
            });
            services.AddRazorPages(options =>
            {
                // Register Razor Page endpoints with lowercase templates (e.g. /txttools/wordcount).
                options.Conventions.AddFolderRouteModelConvention("/", model =>
                {
                    foreach (var selector in model.Selectors)
                    {
                        var route = selector.AttributeRouteModel;
                        if (route?.Template != null)
                        {
                            route.Template = route.Template.ToLowerInvariant();
                        }
                    }
                });
            });
            services.Configure<SeoOptions>(Configuration.GetSection("Seo"));
            //?????????
            services.AddLocalization(t => {
                t.ResourcesPath = "Resources";
            });
            services.AddMvc().AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix).AddDataAnnotationsLocalization();
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    "en",
                    "zh"
                };
                options.SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
            });
            //fluuentui
            services.AddFluentUIComponents(options =>
            {
                options.ValidateClassNames = false;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }
            //??????????????
            var localizationOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(localizationOptions.Value);

            app.UseForwardedHeaders();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/robots.txt", async context =>
                {
                    var seo = context.RequestServices.GetRequiredService<IOptions<SeoOptions>>().Value;
                    var baseUrl = ResolvePublicBaseUrl(context, seo);
                    var sb = new StringBuilder();
                    sb.AppendLine("User-agent: *");
                    sb.AppendLine("Allow: /");
                    sb.AppendLine("Allow: /llms.txt");
                    if (!string.IsNullOrEmpty(baseUrl))
                    {
                        sb.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");
                        sb.AppendLine($"# LLM-friendly site summary: {baseUrl}/llms.txt");
                    }
                    context.Response.ContentType = "text/plain; charset=utf-8";
                    await context.Response.WriteAsync(sb.ToString());
                });
                endpoints.MapGet("/sitemap.xml", async context =>
                {
                    var seo = context.RequestServices.GetRequiredService<IOptions<SeoOptions>>().Value;
                    var baseUrl = ResolvePublicBaseUrl(context, seo);
                    if (string.IsNullOrEmpty(baseUrl))
                    {
                        context.Response.StatusCode = 404;
                        return;
                    }

                    // Prefer catalog (all tools). Optional Seo:SitemapPaths appends extras only.
                    var paths = ToolSeoCatalog.SitemapPaths.ToList();
                    if (seo.SitemapPaths != null)
                    {
                        foreach (var extra in seo.SitemapPaths)
                        {
                            if (string.IsNullOrWhiteSpace(extra)) continue;
                            var p = extra.StartsWith("/", StringComparison.Ordinal) ? extra : "/" + extra;
                            if (!paths.Contains(p, StringComparer.OrdinalIgnoreCase))
                                paths.Add(p);
                        }
                    }

                    XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
                    var urlset = new XElement(ns + "urlset",
                        paths.Distinct(StringComparer.OrdinalIgnoreCase).Select(p =>
                        {
                            var path = p.StartsWith("/", StringComparison.Ordinal) ? p : "/" + p;
                            var loc = path == "/" ? baseUrl + "/" : baseUrl + path;
                            return new XElement(ns + "url",
                                new XElement(ns + "loc", loc),
                                new XElement(ns + "changefreq", path == "/" ? "daily" : "weekly"),
                                new XElement(ns + "priority", path == "/" ? "1.0" : "0.8"));
                        }));
                    context.Response.ContentType = "application/xml; charset=utf-8";
                    await context.Response.WriteAsync(
                        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine
                        + urlset.ToString(SaveOptions.DisableFormatting));
                });
                endpoints.MapRazorPages();
                // ???Blazor????
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private static string ResolvePublicBaseUrl(HttpContext context, SeoOptions seo)
        {
            if (!string.IsNullOrWhiteSpace(seo.PublicBaseUrl))
                return seo.PublicBaseUrl.TrimEnd('/');

            var host = context.Request.Host.Host;
            // Never advertise localhost / 127.0.0.1 in robots or sitemap.
            if (IsNonPublicHost(host))
                return "https://toolapp.org";

            return $"{context.Request.Scheme}://{context.Request.Host.Value}{context.Request.PathBase}".TrimEnd('/');
        }

        private static bool IsNonPublicHost(string? host)
        {
            if (string.IsNullOrWhiteSpace(host))
                return true;
            if (host.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                return true;
            if (host.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
                return true;
            if (System.Net.IPAddress.TryParse(host, out var ip)
                && (System.Net.IPAddress.IsLoopback(ip) || ip.IsIPv6LinkLocal))
                return true;
            return false;
        }
    }
}
