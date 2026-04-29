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
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.FluentUI.AspNetCore.Components;

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
            services.AddServerSideBlazor();
            services.AddHttpClient();
            services.Configure<FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = 22 * 1024 * 1024;
            });
            services.AddRazorPages();
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
            //???ùù?????ùù??
            var localizationOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(localizationOptions.Value);

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
                    if (!string.IsNullOrEmpty(baseUrl))
                        sb.AppendLine($"Sitemap: {baseUrl}/sitemap.xml");
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
                    var paths = seo.SitemapPaths != null && seo.SitemapPaths.Length > 0
                        ? seo.SitemapPaths
                        : new[] { "/", "/About", "/Donation", "/Weiapp" };
                    XNamespace ns = "http://www.sitemaps.org/schemas/sitemap/0.9";
                    var urlset = new XElement(ns + "urlset",
                        paths.Distinct(StringComparer.OrdinalIgnoreCase).Select(p =>
                        {
                            var path = p.StartsWith("/", StringComparison.Ordinal) ? p : "/" + p;
                            return new XElement(ns + "url",
                                new XElement(ns + "loc", baseUrl + path));
                        }));
                    context.Response.ContentType = "application/xml; charset=utf-8";
                    await context.Response.WriteAsync(
                        "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine
                        + urlset.ToString(SaveOptions.DisableFormatting));
                });
                endpoints.MapRazorPages();
                // ???Blazorùù??
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
                endpoints.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private static string ResolvePublicBaseUrl(HttpContext context, SeoOptions seo)
        {
            if (!string.IsNullOrWhiteSpace(seo.PublicBaseUrl))
                return seo.PublicBaseUrl.TrimEnd('/');
            return $"{context.Request.Scheme}://{context.Request.Host.Value}{context.Request.PathBase}".TrimEnd('/');
        }
    }
}
