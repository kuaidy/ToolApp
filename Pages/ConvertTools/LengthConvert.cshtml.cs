using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ToolApp.Pages
{
    public class LengthConvertModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public LengthConvertModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            ViewData["Content"]=FileHelp.GetFile(@"C:\Program Files\Git\myproject\ToolApp\Content\特殊符号.md");
        }
    }
}
