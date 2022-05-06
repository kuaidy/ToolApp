#pragma checksum "C:\Myproject\ToolApp\Pages\BinaryConversion.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "f33fec2d1c5756c73cc6678f5e6148b191212b19"
// <auto-generated/>
#pragma warning disable 1591
[assembly: global::Microsoft.AspNetCore.Razor.Hosting.RazorCompiledItemAttribute(typeof(ToolApp.Pages.Pages_BinaryConversion), @"mvc.1.0.razor-page", @"/Pages/BinaryConversion.cshtml")]
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
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"f33fec2d1c5756c73cc6678f5e6148b191212b19", @"/Pages/BinaryConversion.cshtml")]
    [global::Microsoft.AspNetCore.Razor.Hosting.RazorSourceChecksumAttribute(@"SHA1", @"a5712624ab805f6fbde6dd373e7cd3a04d8b8bf6", @"/Pages/_ViewImports.cshtml")]
    public class Pages_BinaryConversion : global::Microsoft.AspNetCore.Mvc.RazorPages.Page
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#nullable restore
#line 3 "C:\Myproject\ToolApp\Pages\BinaryConversion.cshtml"
  
    ViewData["Title"] = "进制转换";

#line default
#line hidden
#nullable disable
            WriteLiteral("<div class=\"text-left\">\r\n    <h3>");
#nullable restore
#line 7 "C:\Myproject\ToolApp\Pages\BinaryConversion.cshtml"
   Write(ViewData["Title"]);

#line default
#line hidden
#nullable disable
            WriteLiteral(@"</h3>
    <ul>
        <li class=""li_BConversion"">
            <span>2进制</span>
            <input type=""text"" id=""convert2"" data-type=""2"" oninput=""BinaryConvert2()"">
        </li>
        <li class=""li_BConversion"">
            <span>4进制</span>
            <input type=""text"" id=""convert4"" data-type=""4"" oninput=""BinaryConvert4()"">
        </li>
        <li class=""li_BConversion"">
            <span>8进制</span>
            <input type=""text"" id=""convert8"" data-type=""8"" oninput=""BinaryConvert8()"">
        </li>
        <li class=""li_BConversion"">
            <span>10进制</span>
            <input type=""text"" id=""convert10"" data-type=""8"" oninput=""BinaryConvert10()"">
        </li>
        <li class=""li_BConversion"">
            <span>16进制</span>
            <input type=""text"" id=""convert16"" data-type=""16"" oninput=""BinaryConvert16()"">
        </li>
        <li>
            <button type=""button"" onclick=""ClearAll()"">清空</button>
        </li>
    </ul>
    <div>
        <strong>使用说明</strong>
");
            WriteLiteral(@"        <p>
            在对应的输入框中输入值，即可进行转换。
        </p>
        <strong>相关知识</strong>
        <p>
            进制也就是进位计数制，是人为定义的带进位的计数方法（有不带进位的计数方法，比如原始的结绳计数法，唱票时常用的“正”字计数法，以及类似的tally mark计数）。 对于任何一种进制---X进制，就表示每一位置上的数运算时都是逢X进一位。 十进制是逢十进一，十六进制是逢十六进一，二进制就是逢二进一，以此类推，x进制就是逢x进位。
        </p>
    </div>
</div>

<script>
    $obj_convert2=document.getElementById(""convert2"");
    $obj_convert4=document.getElementById(""convert4"");
    $obj_convert8=document.getElementById(""convert8"");
    $obj_convert10=document.getElementById(""convert10"");
    $obj_convert16=document.getElementById(""convert16"");

    function BinaryConvert2()
    {
        $obj_convert4.value=parseInt($obj_convert2.value,2).toString(4);  
        $obj_convert8.value=parseInt($obj_convert2.value,2).toString(8);  
        $obj_convert10.value=parseInt($obj_convert2.value,2).toString(10);  
        $obj_convert16.value=parseInt($obj_convert2.value,2).toString(16);     
    }

    function BinaryConvert4(){
        $obj_convert");
            WriteLiteral(@"2.value=parseInt($obj_convert4.value,4).toString(2);  
        $obj_convert8.value=parseInt($obj_convert4.value,4).toString(8);  
        $obj_convert10.value=parseInt($obj_convert4.value,4).toString(10);  
        $obj_convert16.value=parseInt($obj_convert4.value,4).toString(16);     
    }

    function BinaryConvert8(){
        $obj_convert2.value=parseInt($obj_convert8.value,8).toString(2);  
        $obj_convert4.value=parseInt($obj_convert8.value,8).toString(4);  
        $obj_convert10.value=parseInt($obj_convert8.value,8).toString(10);  
        $obj_convert16.value=parseInt($obj_convert8.value,8).toString(16);  
    }

    function BinaryConvert10(){
        $obj_convert2.value=parseInt($obj_convert10.value,10).toString(2);  
        $obj_convert4.value=parseInt($obj_convert10.value,10).toString(4);  
        $obj_convert8.value=parseInt($obj_convert10.value,10).toString(8);  
        $obj_convert16.value=parseInt($obj_convert10.value,10).toString(16);
    }

    function BinaryCo");
            WriteLiteral(@"nvert16(){
        $obj_convert2.value=parseInt($obj_convert16.value,16).toString(2);  
        $obj_convert4.value=parseInt($obj_convert16.value,16).toString(4);  
        $obj_convert8.value=parseInt($obj_convert16.value,16).toString(8);  
        $obj_convert10.value=parseInt($obj_convert16.value,16).toString(10);       
    }

    function ClearAll()
    {
        $obj_convert2.value="""";
        $obj_convert4.value="""";
        $obj_convert8.value="""";
        $obj_convert10.value="""";
        $obj_convert16.value=""""; 
    }

</script>
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
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<BinaryConversionModel> Html { get; private set; }
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<BinaryConversionModel> ViewData => (global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary<BinaryConversionModel>)PageContext?.ViewData;
        public BinaryConversionModel Model => ViewData.Model;
    }
}
#pragma warning restore 1591
