#pragma checksum "C:\Program Files\Git\myproject\ToolApp\Pages\Article.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "df5d7004086150a3d49695ca09295698ee3acda5"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(ToolApp.Pages.Pages_Article), @"mvc.1.0.razor-page", @"/Pages/Article.cshtml")]
namespace ToolApp.Pages
{
    #line hidden
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
#nullable restore
#line 1 "C:\Program Files\Git\myproject\ToolApp\Pages\_ViewImports.cshtml"
using ToolApp;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"df5d7004086150a3d49695ca09295698ee3acda5", @"/Pages/Article.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"a5712624ab805f6fbde6dd373e7cd3a04d8b8bf6", @"/Pages/_ViewImports.cshtml")]
    public class Pages_Article : global::Microsoft.AspNetCore.Mvc.RazorPages.Page
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral("<div class=\"text-left\">\r\n    <div id=\"content_view\">      \r\n        <textarea id=\"content_editor\" style=\"display:none;\">");
#nullable restore
#line 5 "C:\Program Files\Git\myproject\ToolApp\Pages\Article.cshtml"
                                                       Write(ViewData["Content"]);

#line default
#line hidden
#nullable disable
            WriteLiteral("</textarea>\r\n    </div>\r\n</div>");
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<ArticleModel> Html { get; private set; }
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<ArticleModel> ViewData => (global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<ArticleModel>)PageContext?.ViewData;
        public ArticleModel Model => ViewData.Model;
    }
}
#pragma warning restore 1591
