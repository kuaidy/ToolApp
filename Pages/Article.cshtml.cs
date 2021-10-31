using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ToolApp.Pages
{
    public class ArticleModel : PageModel
    {
        private readonly ILogger<BinaryConversionModel> _logger;

        public ArticleModel(ILogger<BinaryConversionModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(string filename)
        {
            string apppath=System.IO.Directory.GetCurrentDirectory();
            ViewData["Content"]=apppath;
            // ViewData["Content"]=FileHelp.GetFile(apppath+@"\Content\"+filename);
        }
    }
}
