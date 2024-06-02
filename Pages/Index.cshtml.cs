using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace ToolApp.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        //private readonly IStringLocalizer<IndexModel> _stringLocalizer;

        public IndexModel(ILogger<IndexModel> logger,IStringLocalizer<IndexModel> stringLocalizer)
        {
            _logger = logger;
            //_stringLocalizer = stringLocalizer;
        }
        public void OnGet()
        {
            //ViewData["Common Comparison Table"] = _stringLocalizer["Common Comparison Table"].Value;
            //ViewData["Conversion Tool"] = _stringLocalizer["Conversion Tool"].Value;
            //ViewData["Text Tool"] = _stringLocalizer["Text Tool"].Value;
            //ViewData["Regular Expression"] = _stringLocalizer["Regular Expression"].Value;
            //ViewData["Encoding And Decoding"] = _stringLocalizer["Encoding And Decoding"].Value;
            //ViewData["Guid Generation"] = _stringLocalizer["Guid Generation"].Value;
            //ViewData["QRCode Generation"] = _stringLocalizer["QRCode Generation"].Value;
            //ViewData["Simple Main Image Generation"] = _stringLocalizer["Simple Main Image Generation"].Value;
            //ViewData["Video Embedding Code Generation"] = _stringLocalizer["Video Embedding Code Generation"].Value;
            //ViewData["Product Card Generation"] = _stringLocalizer["Product Card Generation"].Value;
            //ViewData["Ip Query"] = _stringLocalizer["Ip Query"].Value;
            //ViewData["One Click Download Of Images"] = _stringLocalizer["One Click Download Of Images"].Value;
            //ViewData["Coupon Inquiry"] = _stringLocalizer["Coupon Inquiry"].Value;
            //ViewData["Encoding Conversion"] = _stringLocalizer["Encoding Conversion"].Value;
            //ViewData["Website Link Detection Tool"] = _stringLocalizer["Website Link Detection Tool"].Value;
            //ViewData["Common Mime Type Reference Table"] = _stringLocalizer["Common Mime Type Reference Table"].Value;
            //ViewData["CSS Selector reference table"] = _stringLocalizer["CSS Selector reference table"].Value;
            //ViewData["Vim Command Reference Table"] = _stringLocalizer["Vim Command Reference Table"].Value;
            //ViewData["Git Command Reference Table"] = _stringLocalizer["Git Command Reference Table"].Value;
            //ViewData["Emoji"] = _stringLocalizer["Emoji"].Value;
            //ViewData["Common Linux commands"] = _stringLocalizer["Common Linux commands"].Value;
            //ViewData["Special Symbols"] = _stringLocalizer["Special Symbols"].Value;
            //ViewData["HTTP status code"] = _stringLocalizer["HTTP status code"].Value;

            //ViewData["Url Escape Character"] = _stringLocalizer["Url Escape Character"].Value;
            //ViewData["Generating tools"] = _stringLocalizer["Generating tools"].Value;
            //ViewData["Frequently Used Phone List"] = _stringLocalizer["Frequently Used Phone List"].Value;
            //ViewData["ASCII Character Reference"] = _stringLocalizer["ASCII Character Reference"].Value;
            //ViewData["Query tools"] = _stringLocalizer["Query tools"].Value;
            //ViewData["Base conversion"] = _stringLocalizer["Base conversion"].Value;
            //ViewData["Color value conversion"] = _stringLocalizer["Color value conversion"].Value;
            //ViewData["Word count"] = _stringLocalizer["Word count"].Value;
            //ViewData["Json formatting"] = _stringLocalizer["Json formatting"].Value;
            //ViewData["Markdown Editor"] = _stringLocalizer["Markdown Editor"].Value;
            //ViewData["ToolOnline"] = _stringLocalizer["ToolOnline"].Value;
        }
    }
}
