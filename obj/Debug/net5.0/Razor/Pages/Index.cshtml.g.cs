#pragma checksum "C:\Myproject\ToolApp\Pages\Index.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "94fd502eff3031fbbf8ce71ca59beb7b66beb8f2"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(ToolApp.Pages.Pages_Index), @"mvc.1.0.razor-page", @"/Pages/Index.cshtml")]
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
#line 1 "C:\Myproject\ToolApp\Pages\_ViewImports.cshtml"
using ToolApp;

#line default
#line hidden
#nullable disable
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"94fd502eff3031fbbf8ce71ca59beb7b66beb8f2", @"/Pages/Index.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"a5712624ab805f6fbde6dd373e7cd3a04d8b8bf6", @"/Pages/_ViewImports.cshtml")]
    public class Pages_Index : global::Microsoft.AspNetCore.Mvc.RazorPages.Page
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line 3 "C:\Myproject\ToolApp\Pages\Index.cshtml"
  
    ViewData["Title"] = "在线工具";

#line default
#line hidden
#nullable disable
            WriteLiteral(@"
<div class=""text-center"">
    <div>
        <div class=""TypeTitle"">转换工具</div>
        <ul class=""ul_tools"">
            <li class=""li_tool"">
                <a href=""/BinaryConversion"" class=""a_tool"">
                    <svg class=""icon"" aria-hidden=""true"">
                        <use xlink:href=""#icon-binarycode""></use>
                    </svg>
                    <span class=""sp_toolname"">进制转换</span>
               </a>
            </li>
            <li class=""li_tool"">
                <a href=""/ColorConvert"" class=""a_tool"">
                    <svg class=""icon"" aria-hidden=""true"">
                        <use xlink:href=""#icon-dixiayanse""></use>
                    </svg>
                    <span class=""sp_toolname"">颜色值转换</span>
                </a>
            </li> 
        </ul>
    </div>
    
    <div>
        <div class=""TypeTitle"">文本工具</div>
        <ul class=""ul_tools"">
            <li class=""li_tool"">
                <a href=""/JsonFormat"" class=""a_tool"">
        ");
            WriteLiteral(@"            <svg class=""icon"" aria-hidden=""true"">
                        <use xlink:href=""#icon-json""></use>
                    </svg>
                    <span class=""sp_toolname"">Json格式化</span>
                </a>
            </li>
            <li class=""li_tool"">
                <a href=""/MarkdownEdit"" class=""a_tool"">
                    <svg class=""icon"" aria-hidden=""true"">
                        <use xlink:href=""#icon-md""></use>
                    </svg>
                    <span class=""sp_toolname"">Markdown编辑</span>
                </a>
            </li>
            <li class=""li_tool"">
                <a href=""/Regular"" class=""a_tool"">
                    <svg class=""icon"" aria-hidden=""true"">
                        <use xlink:href=""#icon-a-regex1x""></use>
                    </svg>
                    <span class=""sp_toolname"">正则表达式</span>
                </a>
            </li>     
        </ul>
    </div>

    <div>
        <div class=""TypeTitle"">编码转换</div>
        <u");
            WriteLiteral(@"l class=""ul_tools"">
            <li class=""li_tool"">
                <a href=""/EncodeDecode"" class=""a_tool"">
                    <svg class=""icon"" aria-hidden=""true"">
                        <use xlink:href=""#icon-bianma""></use>
                    </svg>
                    <span class=""sp_toolname"">编码解码</span>
               </a>
            </li>
        </ul>
    </div>

    <div>
        <div class=""TypeTitle"">生成工具</div>
        <ul class=""ul_tools"">
            <li class=""li_tool"">
                <a href=""/GuidCreate"" class=""a_tool"">
                    <svg class=""icon"" aria-hidden=""true"">
                        <use xlink:href=""#icon-g""></use>
                    </svg>
                    <span class=""sp_toolname"">Guid生成</span>
               </a>
            </li>
            <li class=""li_tool"">
                <a href=""/QrCodeCreate"" class=""a_tool"">
                    <svg class=""icon"" aria-hidden=""true"">
                        <use xlink:href=""#icon-erweima""></use>
");
            WriteLiteral(@"                    </svg>
                    <span class=""sp_toolname"">二维码生成</span>
               </a>
            </li>
            <li class=""li_tool"">
                <a href=""http://backimg.cn/"" class=""a_tool"">
                    <svg class=""icon"" aria-hidden=""true"">
                        <use xlink:href=""#icon-tupian""></use>
                    </svg>
                    <span class=""sp_toolname"">简单主图生成</span>
               </a>
            </li>
            <li class=""li_tool"">
                <a href=""/CodeGeneration"" class=""a_tool"">
                    <svg class=""icon"" aria-hidden=""true"">
                        <use xlink:href=""#icon-tupian""></use>
                    </svg>
                    <span class=""sp_toolname"">代码生成</span>
               </a>
            </li>
        </ul>
    </div>
    <div>
        <div class=""TypeTitle"">查询工具</div>
        <ul class=""ul_tools"">
            <li class=""li_tool"">
                <a href=""/ShowIp"" class=""a_tool"">
          ");
            WriteLiteral(@"          <svg class=""icon"" aria-hidden=""true"">
                        <use xlink:href=""#icon-g""></use>
                    </svg>
                    <span class=""sp_toolname"">Ip查询</span>
               </a>
            </li>
        </ul>
    </div>
</div>
");
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<IndexModel> Html { get; private set; }
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<IndexModel> ViewData => (global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<IndexModel>)PageContext?.ViewData;
        public IndexModel Model => ViewData.Model;
    }
}
#pragma warning restore 1591
