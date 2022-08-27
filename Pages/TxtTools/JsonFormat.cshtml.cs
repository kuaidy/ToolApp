using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ToolApp.Pages
{
    public class JsonFormatModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public JsonFormatModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            string apppath=System.IO.Directory.GetCurrentDirectory();
            ViewData["Content"]=FileHelp.GetFile(apppath+@"\Content\json格式化.md");
        }
    }
}
