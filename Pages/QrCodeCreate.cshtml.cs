using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ToolApp.Pages
{
    public class QrCodeCreateModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public QrCodeCreateModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            string apppath=System.IO.Directory.GetCurrentDirectory();
            ViewData["Content"]=FileHelp.GetFile(apppath+@"\Content\Url二维码.md");
        }
    }
}
